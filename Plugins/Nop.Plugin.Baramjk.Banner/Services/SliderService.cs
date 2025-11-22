using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Nop.Core.Events;
using Nop.Data;
using Nop.Plugin.Baramjk.Banner.Domain.AnywhereSliders;

namespace Nop.Plugin.Baramjk.Banner.Services
{
    public class SliderService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<Slide> _slideRepository;
        private readonly IRepository<Slider> _sliderRepository;

        public SliderService(IEventPublisher eventPublisher, IRepository<Slide> slideRepository,
            IRepository<Slider> sliderRepository)
        {
            _eventPublisher = eventPublisher;
            _slideRepository = slideRepository;
            _sliderRepository = sliderRepository;
        }

        public async Task<List<Slider>> GetAllSlidersAsync()
        {
            return await _sliderRepository.Table.ToListAsync();
        }

        public async Task<IList<Slider>> GetSlidersByWidgetZoneAsync(string widgetZone)
        {
            return await _sliderRepository.Table.ToListAsync();
        }

        public async Task<Slider> GetSliderByIdAsync(int id)
        {
            return await _sliderRepository.GetByIdAsync(id);
        }

        public async Task<Slider> GetAvailableSliderByIdAsync(int id)
        {
            return await _sliderRepository.Table.FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<Slider> GetAvailableSliderBySystemNameAsync(string systemName)
        {
            return await _sliderRepository.Table.Where(
                (Expression<Func<Slider, bool>>)(s => s.SystemName == systemName)).FirstOrDefaultAsync();
        }

        public async Task InsertSliderAsync(Slider slider)
        {
            if (slider == null)
                throw new ArgumentNullException(nameof(slider));
            await _sliderRepository.InsertAsync(slider);
            await _eventPublisher.EntityInsertedAsync(slider);
        }

        public async Task UpdateSliderAsync(Slider slider)
        {
            if (slider == null)
                throw new ArgumentNullException(nameof(slider));
            await _sliderRepository.UpdateAsync(slider);
            await _eventPublisher.EntityUpdatedAsync(slider);
        }

        public async Task DeleteSliderAsync(Slider slider)
        {
            if (slider == null)
                throw new ArgumentNullException(nameof(slider));
            await _sliderRepository.DeleteAsync(slider);
            await _eventPublisher.EntityDeletedAsync(slider);
        }

        public async Task<IList<Slide>> GetAllSlidesBySliderIdAsync(int sliderId)
        {
            return await _slideRepository
                .Table.Where((Expression<Func<Slide, bool>>)(i => i.SliderId == sliderId))
                .OrderBy((Expression<Func<Slide, int>>)(i => i.DisplayOrder)).ToListAsync();
        }

        public async Task<IList<Slide>> GetAllVisibleSlidesAsync(int sliderId)
        {
            return await _slideRepository
                .Table.Where((Expression<Func<Slide, bool>>)(i => i.SliderId == sliderId && i.Visible))
                .OrderBy((Expression<Func<Slide, int>>)(i => i.DisplayOrder)).ToListAsync();
        }

        public async Task<IList<Slide>> GetAllSlidesAsync()
        {
            return await _slideRepository.Table.ToListAsync();
        }

        public async Task<Slide> GetSlideByIdAsync(int slideId)
        {
            return await _slideRepository.GetByIdAsync(slideId);
        }

        public async Task InsertSlideAsync(Slide slide)
        {
            if (slide == null)
                throw new ArgumentNullException(nameof(slide));
            await _slideRepository.InsertAsync(slide);
            await _eventPublisher.EntityInsertedAsync(slide);
        }

        public async Task UpdateSlideAsync(Slide slide)
        {
            if (slide == null)
                throw new ArgumentNullException(nameof(slide));
            await _slideRepository.UpdateAsync(slide);
            await _eventPublisher.EntityUpdatedAsync(slide);
        }

        public async Task DeleteSlideAsync(Slide slide)
        {
            if (slide == null)
                throw new ArgumentNullException(nameof(slide));
            await _slideRepository.DeleteAsync(slide);
            await _eventPublisher.EntityDeletedAsync(slide);
        }
    }
}