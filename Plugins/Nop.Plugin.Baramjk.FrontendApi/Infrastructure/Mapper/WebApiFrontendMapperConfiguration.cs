using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Baramjk.FrontendApi.Dto;
using Nop.Plugin.Baramjk.FrontendApi.Dto.BackInStockSubscription;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Blog;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Boards;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Catalog;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Checkout;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Common;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Country;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Currencies;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Customer;
using Nop.Plugin.Baramjk.FrontendApi.Dto.News;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Newsletter;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Orders;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Poll;
using Nop.Plugin.Baramjk.FrontendApi.Dto.PrivateMessages;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Product;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Profiles;
using Nop.Plugin.Baramjk.FrontendApi.Dto.ReturnRequests;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Seo;
using Nop.Plugin.Baramjk.FrontendApi.Dto.ShoppingCart;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Topics;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Vendors;
using Nop.Plugin.Baramjk.FrontendApi.Dto.Wishlist;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Infrastructure.Mapper;
using Nop.Web.Framework.UI.Paging;
using Nop.Web.Models.Blogs;
using Nop.Web.Models.Boards;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using Nop.Web.Models.Directory;
using Nop.Web.Models.Media;
using Nop.Web.Models.News;
using Nop.Web.Models.Newsletter;
using Nop.Web.Models.Order;
using Nop.Web.Models.Polls;
using Nop.Web.Models.PrivateMessages;
using Nop.Web.Models.Profile;
using Nop.Web.Models.ShoppingCart;
using Nop.Web.Models.Topics;
using Nop.Web.Models.Vendors;
using SearchModel = Nop.Web.Models.Catalog.SearchModel;

namespace Nop.Plugin.Baramjk.FrontendApi.Infrastructure.Mapper
{
    /// <summary>
    ///     AutoMapper configuration for frontend Web API DTO
    /// </summary>
    public class WebApiFrontendMapperConfiguration : BaseMapperConfiguration
    {
        #region Ctor

        public WebApiFrontendMapperConfiguration()
        {
            //create specific maps
            CreateTopicsMaps();
            CreateVendorMaps();
            CreateProfileMaps();
            CreateReturnRequestsMaps();
            CreateOrdersMaps();
            CreatePrivateMessageMaps();
            CreatePollMaps();
            CreateNewsletterMaps();
            CreateCommonMaps();
            CreateBaseModelsMaps();
            CreateNewsMaps();
            CreateCountryMaps();
            CreateBackInStockSubscriptionMaps();
            CreateProductMaps();
            CreateShoppingCartMaps();
            CreateCategoryMaps();
            CreateBoardsMaps();
            CreateWishlistMaps();
            CreateCustomerMaps();
            CreateBlogMaps();
            CreateCheckoutMaps();
            CreateUrlRecordMaps();
            CreateCurrenciesMaps();
        }

        #endregion

        #region Utilites

        /// <summary>
        ///     Create topics maps
        /// </summary>
        protected virtual void CreateTopicsMaps()
        {
            CreateDtoMap<TopicModel, TopicModelDto>();
        }
        
        /// <summary>
        ///     Create vendor maps
        /// </summary>
        protected virtual void CreateVendorMaps()
        {
            CreateDtoMap<VendorInfoModel, VendorInfoModelDto>();
            CreateDtoMap<VendorAttributeModel, VendorAttributeModelDto>();
            CreateDtoMap<VendorAttributeValueModel, VendorAttributeValueModelDto>();
            CreateDtoMap<ApplyVendorModel, ApplyVendorModelDto>();
            CreateDtoMap<VendorAttribute, VendorAttributeModelDto>();
            CreateDtoMap<VendorAttributeValue, VendorAttributeValueModelDto>();
        }

        /// <summary>
        ///     Create profile maps
        /// </summary>
        protected virtual void CreateProfileMaps()
        {
            CreateDtoMap<ProfileIndexModel, ProfileIndexModelDto>();
        }

        /// <summary>
        ///     Create return request maps
        /// </summary>
        protected virtual void CreateReturnRequestsMaps()
        {
            CreateDtoMap<SubmitReturnRequestModel, SubmitReturnRequestModelDto>();
            CreateDtoMap<CustomerReturnRequestsModel, CustomerReturnRequestsModelDto>();
        }

        /// <summary>
        ///     Create orders maps
        /// </summary>
        protected virtual void CreateOrdersMaps()
        {
            CreateDtoMap<CustomerReturnRequestsModel.ReturnRequestModel, ReturnRequestModelDto>();
            CreateDtoMap<SubmitReturnRequestModel.ReturnRequestActionModel, ReturnRequestActionModelDto>();
            CreateDtoMap<SubmitReturnRequestModel.ReturnRequestReasonModel, ReturnRequestReasonModelDto>();
            CreateDtoMap<SubmitReturnRequestModel.OrderItemModel,
                SubmitReturnRequestModelDto.ReturnRequestOrderItemModelDto>();
            CreateDtoMap<ShipmentDetailsModel.ShipmentStatusEventModel, ShipmentStatusEventModelDto>();
            CreateDtoMap<ShipmentDetailsModel.ShipmentItemModel, ShipmentItemModelDto>();
            CreateDtoMap<ShipmentDetailsModel, ShipmentDetailsModelDto>();
            CreateDtoMap<CustomerRewardPointsModel.RewardPointsHistoryModel, RewardPointsHistoryModelDto>();
            CreateDtoMap<CustomerOrderListModel, CustomerOrderListModelDto>();
            CreateDtoMap<CustomerRewardPointsModel, CustomerRewardPointsModelDto>();
            CreateDtoMap<CustomerOrderListModel.RecurringOrderModel, RecurringOrderModelDto>();
            CreateDtoMap<CustomerOrderListModel.OrderDetailsModel,
                CustomerOrderListModelDto.CustomerOrderDetailsModelDto>();
            CreateDtoMapping<OrderDetailsModel, OrderDetailsModelDto>();

            CreateDtoMapping<OrderDetailsModel.OrderItemModel, OrderDetailsModelDto.OrderItemModelDto>();
            CreateDtoMapping<OrderDetailsModel.TaxRate, OrderDetailsModelDto.OrderDetailsTaxRateDto>();
            CreateDtoMapping<OrderDetailsModel.GiftCard, OrderDetailsModelDto.OrderDetailsGiftCardDto>();
            CreateDtoMapping<OrderDetailsModel.OrderNote, OrderDetailsModelDto.OrderNoteDto>();
            CreateDtoMapping<OrderDetailsModel.ShipmentBriefModel, OrderDetailsModelDto.ShipmentBriefModelDto>();
        }

        /// <summary>
        ///     Create private message maps
        /// </summary>
        protected virtual void CreatePrivateMessageMaps()
        {
            CreateDtoMap<SendPrivateMessageModel, SendPrivateMessageModelDto>();
            CreateDtoMap<PrivateMessageIndexModel, PrivateMessageIndexModelDto>();
            CreateDtoMap<SubmitReturnRequestModel, SubmitReturnRequestModelDto>();
            CreateDtoMap<PrivateMessageModel, PrivateMessageModelDto>();
            CreateDtoMap<CustomerReturnRequestsModel, CustomerReturnRequestsModelDto>();
        }

        /// <summary>
        ///     Create poll maps
        /// </summary>
        protected virtual void CreatePollMaps()
        {
            CreateDtoMap<PollAnswerModel, PollAnswerModelDto>();
            CreateDtoMap<PollModel, PollModelDto>();
        }

        /// <summary>
        ///     Create subscription maps
        /// </summary>
        protected virtual void CreateNewsletterMaps()
        {
            CreateDtoMap<SubscriptionActivationModel, SubscriptionActivationModelDto>();
        }

        /// <summary>
        ///     Create common maps
        /// </summary>
        protected virtual void CreateCommonMaps()
        {
            CreateDtoMap<PagerModel, PagerModelDto>();
            CreateDtoMap<IRouteValues, BaseRouteValuesModelDto>();

            CreateDtoMap<ContactUsModel, ContactUsModelDto>();

            CreateDtoMap<ContactVendorModel, ContactVendorModelDto>();

            CreateDtoMap<SitemapPageModel, SitemapPageModelDto>();

            CreateDtoMap<SitemapModel, SitemapModelDto>();

            CreateDtoMap<SitemapModel.SitemapItemModel, SitemapItemModelDto>();

            CreateDtoMap<SelectListItem, SelectListItemDto>();
            CreateDtoMap<SelectListGroup, SelectListGroupDto>();

            CreateDtoMap<AddressAttributeValueModel, AddressAttributeValueModelDto>();

            CreateDtoMap<AddressAttributeModel, AddressAttributeModelDto>();

            CreateDtoMap<AddressModel, AddressModelDto>();
        }

        /// <summary>
        ///     Create base models maps
        /// </summary>
        protected virtual void CreateBaseModelsMaps()
        {
            CreateDtoMap<BasePageableModel, BasePageableModelDto>();
        }

        /// <summary>
        ///     Create news maps
        /// </summary>
        protected virtual void CreateNewsMaps()
        {
            CreateDtoMap<AddNewsCommentModel, AddNewsCommentModelDto>();
            CreateDtoMap<NewsCommentModel, NewsCommentModelDto>();
            CreateDtoMap<NewsItemListModel, NewsItemListModelDto>();
            CreateDtoMap<NewsItemModel, NewsItemModelDto>();
        }

        /// <summary>
        ///     Create country maps
        /// </summary>
        protected virtual void CreateCountryMaps()
        {
            CreateDtoMap<StateProvinceModel, StateProvinceModelDto>();
            CreateMap<Country, CountryDto>();
        }

        /// <summary>
        ///     Create back in stock subscription maps
        /// </summary>
        protected virtual void CreateBackInStockSubscriptionMaps()
        {
            CreateDtoMap<BackInStockSubscribeModel, BackInStockSubscribeModelDto>();
            CreateDtoMap<CustomerBackInStockSubscriptionsModel, CustomerBackInStockSubscriptionsModelDto>();
            CreateDtoMap<CustomerBackInStockSubscriptionsModel.BackInStockSubscriptionModel,
                CustomerBackInStockSubscriptionsModelDto.BackInStockSubscriptionModelDto>();
        }

        /// <summary>
        ///     Create product maps
        /// </summary>
        protected virtual void CreateProductMaps()
        {
            CreateDtoMapping<ProductDetailsModel, ProductDetailsModelDto>();
            CreateDtoMap<PictureModel, PictureModelDto>();
            CreateDtoMap<VendorBriefInfoModel, VendorBriefInfoModelDto>();
            CreateDtoMapping<ProductDetailsModel.GiftCardModel, ProductDetailsModelDto.GiftCardModelDto>();
            CreateDtoMapping<ProductDetailsModel.ProductPriceModel, ProductDetailsModelDto.ProductPriceModelDto>();
            CreateDtoMap<ProductDetailsModel.AddToCartModel, AddToCartModelDto>();
            CreateDtoMap<ProductDetailsModel.ProductBreadcrumbModel, ProductBreadcrumbModelDto>();
            CreateDtoMap<CategorySimpleModel, CategorySimpleModelDto>();
            CreateDtoMap<ProductTagModel, ProductTagModelDto>();
            CreateDtoMapping<ProductDetailsModel.ProductAttributeModel,
                ProductDetailsModelDto.ProductDetailsAttributeModelDto>();
            CreateDtoMap<ProductDetailsModel.ProductAttributeValueModel, ProductAttributeValueModelDto>();
            CreateDtoMap<ProductSpecificationModel, ProductSpecificationModelDto>();
            CreateDtoMap<ProductSpecificationAttributeGroupModel, ProductSpecificationAttributeGroupModelDto>();
            CreateDtoMap<ProductSpecificationAttributeModel, ProductSpecificationAttributeModelDto>();
            CreateDtoMap<ProductSpecificationAttributeValueModel, ProductSpecificationAttributeValueModelDto>();
            CreateDtoMap<ManufacturerBriefInfoModel, ManufacturerBriefInfoModelDto>();
            CreateDtoMap<ProductReviewOverviewModel, ProductReviewOverviewModelDto>();
            CreateDtoMap<ProductDetailsModel.ProductEstimateShippingModel, ProductEstimateShippingModelDto>();
            CreateDtoMap<ProductDetailsModel.TierPriceModel, TierPriceModelDto>();

            CreateDtoMap<ProductCombinationModel, ProductCombinationModelDto>();
            CreateDtoMap<ProductAttributeModel, ProductAttributeModelDto>();

            CreateDtoMapping<ProductOverviewModel, ProductOverviewModelDto>();
            CreateDtoMapping<ProductOverviewModel.ProductPriceModel,
                ProductOverviewModelDto.ProductOverviewProductPriceModelDto>();

            CreateDtoMap<ProductReviewsModel, ProductReviewsModelDto>();
            CreateDtoMap<ProductReviewModel, ProductReviewModelDto>();
            CreateDtoMap<ProductReviewHelpfulnessModel, ProductReviewHelpfulnessModelDto>();
            CreateDtoMap<ProductReviewReviewTypeMappingModel, ProductReviewReviewTypeMappingModelDto>();
            CreateDtoMap<AddProductReviewModel, AddProductReviewModelDto>();
            CreateDtoMap<ReviewTypeModel, ReviewTypeModelDto>();
            CreateDtoMap<AddProductReviewReviewTypeMappingModel, AddProductReviewReviewTypeMappingModelDto>();

            CreateDtoMap<CustomerProductReviewsModel, CustomerProductReviewsModelDto>();
            CreateDtoMap<CustomerProductReviewModel, CustomerProductReviewModelDto>();

            CreateDtoMap<ProductEmailAFriendModel, ProductEmailAFriendModelDto>();

            CreateDtoMap<CompareProductsModel, CompareProductsModelDto>();
        }

        /// <summary>
        ///     Create category maps
        /// </summary>
        protected virtual void CreateCategoryMaps()
        {
            CreateDtoMap<CatalogProductsCommand, CatalogProductsCommandDto>();

            CreateDtoMap<CategoryModel, CategoryModelDto>();
            CreateDtoMap<CategoryModel.SubCategoryModel, SubCategoryModelDto>();
            CreateDtoMap<CatalogProductsModel, CatalogProductsModelDto>();
            CreateDtoMap<PriceRangeFilterModel, PriceRangeFilterModelDto>();
            CreateDtoMap<SpecificationFilterModel, SpecificationFilterModelDto>();
            CreateDtoMap<PriceRangeModel, PriceRangeModelDto>();
            CreateDtoMap<SpecificationAttributeFilterModel, SpecificationAttributeFilterModelDto>();
            CreateDtoMap<SpecificationAttributeValueFilterModel, SpecificationAttributeValueFilterModelDto>();
            CreateDtoMap<ManufacturerFilterModel, ManufacturerFilterModelDto>();
            CreateDtoMap<ManufacturerModel, ManufacturerModelDto>();
            CreateDtoMap<VendorModel, VendorModelDto>();
            CreateDtoMap<ProductsByTagModel, ProductsByTagModelDto>();
            CreateDtoMap<PopularProductTagsModel, PopularProductTagsModelDto>();

            CreateDtoMap<SearchModel, SearchModelDto>();
            CreateDtoMap<SearchModel.CategoryModel, SearchCategoryModelDto>();
        }

        /// <summary>
        ///     Create shopping cart maps
        /// </summary>
        protected virtual void CreateShoppingCartMaps()
        {
            CreateDtoMap<EstimateShippingResultModel, EstimateShippingResultModelDto>();
            CreateDtoMap<EstimateShippingResultModel.ShippingOptionModel, ShippingOptionModelDto>();
            CreateDtoMap<ShoppingCartModel.GiftCardBoxModel, GiftCardBoxModelDto>();
            CreateDtoMap<ShoppingCartModel.DiscountBoxModel, DiscountBoxModelDto>();
            CreateDtoMap<ShoppingCartModel.DiscountBoxModel.DiscountInfoModel, DiscountInfoModelDto>();
            CreateDtoMap<ShoppingCartModel.CheckoutAttributeValueModel, CheckoutAttributeValueModelDto>();
            CreateDtoMap<OrderTotalsModel.TaxRate, TaxRateDto>();
            CreateDtoMap<OrderTotalsModel.GiftCard, GiftCardDto>();
            CreateDtoMap<ShoppingCartModel, ShoppingCartModelDto>();
            CreateDtoMap<MiniShoppingCartModel.ShoppingCartItemModel,
                MiniShoppingCartModelDto.MiniShoppingCartItemModelDto>();
            CreateDtoMap<OrderTotalsModel, OrderTotalsModelDto>();
            CreateDtoMap<MiniShoppingCartModel, MiniShoppingCartModelDto>();
            CreateDtoMap<ShoppingCartModel.ShoppingCartItemModel, ShoppingCartItemModelDto>();
            CreateDtoMap<EstimateShippingModel, EstimateShippingModelDto>();
            CreateDtoMap<ShoppingCartModel.CheckoutAttributeModel, CheckoutAttributeModelDto>();
            CreateDtoMap<ShoppingCartModel.OrderReviewDataModel, OrderReviewDataModelDto>();
        }

        /// <summary>
        ///     Create boards maps
        /// </summary>
        protected virtual void CreateBoardsMaps()
        {
            CreateDtoMap<BoardsIndexModel, BoardsIndexModelDto>();
            CreateDtoMap<ForumGroupModel, ForumGroupModelDto>();
            CreateDtoMap<ForumRowModel, ForumRowModelDto>();
            CreateDtoMap<ActiveDiscussionsModel, ActiveDiscussionsModelDto>();
            CreateDtoMap<ForumTopicRowModel, ForumTopicRowModelDto>();
            CreateDtoMap<ForumPageModel, ForumPageModelDto>();
            CreateDtoMap<ForumTopicPageModel, ForumTopicPageModelDto>();
            CreateDtoMap<ForumPostModel, ForumPostModelDto>();
            CreateDtoMap<TopicMoveModel, TopicMoveModelDto>();
            CreateDtoMap<EditForumTopicModel, EditForumTopicModelDto>();

            CreateDtoMap<EditForumPostModel, EditForumPostModelDto>();

            CreateDtoMap<Web.Models.Boards.SearchModel, SearchBoardsModelDto>();
            CreateDtoMap<CustomerForumSubscriptionsModel, CustomerForumSubscriptionsModelDto>();
            CreateDtoMap<CustomerForumSubscriptionsModel.ForumSubscriptionModel, ForumSubscriptionModelDto>();
        }

        /// <summary>
        ///     Create wishlist maps
        /// </summary>
        protected virtual void CreateWishlistMaps()
        {
            CreateDtoMap<WishlistModel.ShoppingCartItemModel, WishlistModelDto.ShoppingCartItemModel>();
            CreateDtoMap<WishlistEmailAFriendModel, WishlistEmailAFriendModelDto>();
            CreateDtoMap<WishlistModel, WishlistModelDto>();
        }

        /// <summary>
        ///     Create customer maps
        /// </summary>
        protected virtual void CreateCustomerMaps()
        {
            CreateDtoMap<PasswordRecoveryModel, PasswordRecoveryModelDto>();

            CreateDtoMap<PasswordRecoveryConfirmModel, PasswordRecoveryConfirmModelDto>();

            CreateDtoMap<RegisterModel, RegisterModelDto>();

            CreateDtoMap<CustomerAttributeModel, CustomerAttributeModelDto>();

            CreateDtoMap<CustomerAttributeValueModel, CustomerAttributeValueModelDto>();

            CreateDtoMap<GdprConsentModel, GdprConsentModelDto>();

            CreateDtoMap<RegisterResultModel, RegisterResultModelDto>();
            CreateDtoMap<AccountActivationModel, AccountActivationModelDto>();

            CreateDtoMap<CustomerInfoModel, CustomerInfoModelDto>();
            CreateDtoMap<CustomerInfoModel, CustomerInfoDto>();

            CreateDtoMap<CustomerInfoModel.AssociatedExternalAuthModel, AssociatedExternalAuthModelDto>();

            CreateDtoMap<EmailRevalidationModel, EmailRevalidationModelDto>();

            CreateDtoMap<CustomerAddressListModel, CustomerAddressListModelDto>();
            CreateDtoMap<CustomerAddressEditModel, CustomerAddressEditModelDto>();

            CreateDtoMap<CustomerDownloadableProductsModel, CustomerDownloadableProductsModelDto>();
            CreateDtoMap<CustomerDownloadableProductsModel.DownloadableProductsModel, DownloadableProductsModelDto>();

            CreateDtoMap<UserAgreementModel, UserAgreementModelDto>();
            CreateDtoMap<ChangePasswordModel, ChangePasswordModelDto>();

            CreateDtoMap<CustomerAvatarModel, CustomerAvatarModelDto>();

            CreateDtoMap<GdprToolsModel, GdprToolsModelDto>();

            CreateDtoMap<CheckGiftCardBalanceModel, CheckGiftCardBalanceModelDto>();
        }

        /// <summary>
        ///     Create blog maps
        /// </summary>
        protected virtual void CreateBlogMaps()
        {
            CreateDtoMap<AddBlogCommentModel, AddBlogCommentModelDto>();
            CreateDtoMap<BlogPostListModel, BlogPostListModelDto>();
            CreateDtoMap<BlogPagingFilteringModel, BlogPagingFilteringModelDto>();
            CreateDtoMap<BlogPostModel, BlogPostModelDto>();
            CreateDtoMap<BlogCommentModel, BlogCommentModelDto>();
        }

        /// <summary>
        ///     Create checkout maps
        /// </summary>
        protected virtual void CreateCheckoutMaps()
        {
            CreateDtoMap<CheckoutCompletedModel, CheckoutCompletedModelDto>();

            CreateDtoMap<CheckoutBillingAddressModel, CheckoutBillingAddressModelDto>();

            CreateDtoMap<CheckoutShippingAddressModel, CheckoutShippingAddressModelDto>();
            CreateDtoMap<CheckoutPickupPointsModel, CheckoutPickupPointsModelDto>();
            CreateDtoMap<CheckoutPickupPointModel, CheckoutPickupPointModelDto>();

            CreateDtoMap<CheckoutShippingMethodModel, CheckoutShippingMethodModelDto>();
            CreateDtoMap<CheckoutShippingMethodModel.ShippingMethodModel, ShippingMethodModelDto>();
            CreateDtoMap<ShippingOption, ShippingOptionDto>();

            CreateDtoMap<CheckoutPaymentMethodModel, CheckoutPaymentMethodModelDto>();
            CreateDtoMap<CheckoutPaymentMethodModel.PaymentMethodModel, PaymentMethodModelDto>();

            CreateDtoMap<CheckoutPaymentInfoModel, CheckoutPaymentInfoModelDto>();
            CreateDtoMap<CheckoutConfirmModel, CheckoutConfirmModelDto>();

            CreateDtoMap<OnePageCheckoutModel, OnePageCheckoutModelDto>();
        }

        /// <summary>
        ///     Create url record maps
        /// </summary>
        protected virtual void CreateUrlRecordMaps()
        {
            CreateDtoMap<UrlRecord, UrlRecordDto>();
        }
        
        protected virtual void CreateCurrenciesMaps()
        {
            CreateMap<Currency, CurrencyDto>();
        }

        #endregion
    }
}