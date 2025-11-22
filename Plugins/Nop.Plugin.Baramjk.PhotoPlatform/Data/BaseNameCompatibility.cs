using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;
using Nop.Plugin.Baramjk.PhotoPlatform.Plugins;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            { typeof(TimeSlot), $"{DefaultValues.TableNamePrefix}{nameof(TimeSlot)}" },
            { typeof(EventDetail), $"{DefaultValues.TableNamePrefix}{nameof(EventDetail)}" },
            { typeof(Actor), $"{DefaultValues.TableNamePrefix}{nameof(Actor)}" },
            { typeof(ActorEvent), $"{DefaultValues.TableNamePrefix}{nameof(ActorEvent)}" },
            { typeof(ShoppingCartItemTimeSlot), $"{DefaultValues.TableNamePrefix}{nameof(ShoppingCartItemTimeSlot)}" },
            { typeof(ReservationItem), $"{DefaultValues.TableNamePrefix}{nameof(ReservationItem)}" },
            { typeof(ReservationHistory), $"{DefaultValues.TableNamePrefix}{nameof(ReservationHistory)}" },
            { typeof(CashierEvent), $"{DefaultValues.TableNamePrefix}{nameof(CashierEvent)}" },
            { typeof(ActorPicture), $"{DefaultValues.TableNamePrefix}{nameof(ActorPicture)}" },
            { typeof(CashierBalanceHistory), $"{DefaultValues.TableNamePrefix}{nameof(CashierBalanceHistory)}" },
            { typeof(ContactInfoEntity), $"{DefaultValues.TableNamePrefix}{nameof(ContactInfoEntity)}" },
            { typeof(SubjectEntity), $"{DefaultValues.TableNamePrefix}{nameof(SubjectEntity)}" },
            { typeof(SupervisorEvent), $"{DefaultValues.TableNamePrefix}{nameof(SupervisorEvent)}" },
            { typeof(ProductionEvent), $"{DefaultValues.TableNamePrefix}{nameof(ProductionEvent)}" },
            { typeof(CashierDailyBalance), $"{DefaultValues.TableNamePrefix}{nameof(CashierDailyBalance)}" },
            { typeof(ActorEventTimeSlot), $"{DefaultValues.TableNamePrefix}{nameof(ActorEventTimeSlot)}" }
        };

        public Dictionary<(Type, string), string> ColumnName => new();
    }
}