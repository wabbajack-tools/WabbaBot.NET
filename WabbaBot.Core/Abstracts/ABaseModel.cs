using System.ComponentModel.DataAnnotations;
using WabbaBot.Interfaces;

namespace WabbaBot.Core.Abstracts {
    public abstract class ABaseModel : IHasId, IHasCreatedModifiedDates {
        [Key]
        public virtual int Id { get; set; }
        public virtual DateTime CreatedOn { get; set; }
        public virtual DateTime ModifiedOn { get; set; }
    }
}
