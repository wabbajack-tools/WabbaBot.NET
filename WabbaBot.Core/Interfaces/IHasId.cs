using System.ComponentModel.DataAnnotations;

namespace WabbaBot.Interfaces {
    public interface IHasId {
        [Key]
        public int Id { get; set; }
    }
}
