using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Baramjk.Framework.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class PhotoPlatformOrderTotalCalculationService : OrderTotalCalculationService
    {
        private readonly PhotoPlatformShoppingCartService _photoPlatformShoppingCartService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;

        public PhotoPlatformOrderTotalCalculationService(CatalogSettings catalogSettings, IAddressService addressService,
            ICheckoutAttributeParser checkoutAttributeParser, ICustomerService customerService, IDiscountService discountService,
            IGenericAttributeService genericAttributeService, IGiftCardService giftCardService, IOrderService orderService,
            IPaymentService paymentService, IPriceCalculationService priceCalculationService, IProductService productService,
            IRewardPointService rewardPointService, IShippingPluginManager shippingPluginManager, IShippingService shippingService,
            IShoppingCartService shoppingCartService, IStoreContext storeContext, ITaxService taxService, IWorkContext workContext,
            RewardPointsSettings rewardPointsSettings, ShippingSettings shippingSettings, ShoppingCartSettings shoppingCartSettings,
            TaxSettings taxSettings, PhotoPlatformShoppingCartService photoPlatformShoppingCartService) : base(catalogSettings, addressService,
            checkoutAttributeParser, customerService, discountService,
            genericAttributeService, giftCardService, orderService, paymentService, priceCalculationService, productService, rewardPointService,
            shippingPluginManager, shippingService, shoppingCartService, storeContext, taxService, workContext, rewardPointsSettings,
            shippingSettings, shoppingCartSettings, taxSettings)
        {
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _priceCalculationService = priceCalculationService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _taxService = taxService;
            _workContext = workContext;
            _rewardPointsSettings = rewardPointsSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _photoPlatformShoppingCartService = photoPlatformShoppingCartService;
        }

        public override async
            Task<(decimal discountAmount, List<Discount> appliedDiscounts, decimal subTotalWithoutDiscount, decimal subTotalWithDiscount,
                SortedDictionary<decimal, decimal> taxRates)> GetShoppingCartSubTotalAsync(IList<ShoppingCartItem> cart, bool includingTax)
        {
            var discountAmount = decimal.Zero;
            var appliedDiscounts = new List<Discount>();
            var subTotalWithoutDiscount = decimal.Zero;
            var subTotalWithDiscount = decimal.Zero;
            var taxRates = new SortedDictionary<decimal, decimal>();

            if (cart.IsEmptyOrNull())
                return (discountAmount, appliedDiscounts, subTotalWithoutDiscount, subTotalWithDiscount, taxRates);

            //get the customer 
            var customer = await _customerService.GetShoppingCartCustomerAsync(cart);

            //sub totals
            var subTotalExclTaxWithoutDiscount = decimal.Zero;

            foreach (var shoppingCartItem in cart)
            {
                var (sciSubTotal, _, _, _) = await _shoppingCartService.GetSubTotalAsync(shoppingCartItem, true);
                var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);

                var (sciExclTax, _) = await _taxService.GetProductPriceAsync(product, sciSubTotal, false, customer);
                subTotalExclTaxWithoutDiscount += sciExclTax;
            }

            //! custom logic -> calculate the photography price
            subTotalExclTaxWithoutDiscount +=
                await _photoPlatformShoppingCartService.CalculateShoppingCartPriceAsync(cart.Select(sci => sci.Id).ToArray());

            //!

            //subtotal without discount
            subTotalWithoutDiscount = subTotalExclTaxWithoutDiscount;
            if (subTotalWithoutDiscount < decimal.Zero)
                subTotalWithoutDiscount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                subTotalWithoutDiscount = await _priceCalculationService.RoundPriceAsync(subTotalWithoutDiscount);

            //We calculate discount amount on order subtotal excl tax (discount first)
            //calculate discount amount ('Applied to order subtotal' discount)
            decimal discountAmountExclTax;
            (discountAmountExclTax, appliedDiscounts) = await GetOrderSubtotalDiscountAsync(customer, subTotalExclTaxWithoutDiscount);
            if (subTotalExclTaxWithoutDiscount < discountAmountExclTax)
                discountAmountExclTax = subTotalExclTaxWithoutDiscount;

            var subTotalExclTaxWithDiscount = subTotalExclTaxWithoutDiscount - discountAmountExclTax;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                discountAmountExclTax = await _priceCalculationService.RoundPriceAsync(discountAmountExclTax);
            }

            subTotalWithDiscount = subTotalExclTaxWithDiscount;
            discountAmount = discountAmountExclTax;

            if (subTotalWithDiscount < decimal.Zero)
                subTotalWithDiscount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                subTotalWithDiscount = await _priceCalculationService.RoundPriceAsync(subTotalWithDiscount);

            return (discountAmount, appliedDiscounts, subTotalWithoutDiscount, subTotalWithDiscount, taxRates);
        }

        public override async
            Task<(decimal? shoppingCartTotal, decimal discountAmount, List<Discount> appliedDiscounts, List<AppliedGiftCard> appliedGiftCards, int
                redeemedRewardPoints, decimal redeemedRewardPointsAmount)> GetShoppingCartTotalAsync(IList<ShoppingCartItem> cart,
                bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true)
        {
            if (_rewardPointsSettings.Enabled)
                useRewardPoints = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute,
                    (await _storeContext.GetCurrentStoreAsync()).Id);


            return await base.GetShoppingCartTotalAsync(cart, useRewardPoints, usePaymentMethodAdditionalFee);
        }
    }
}