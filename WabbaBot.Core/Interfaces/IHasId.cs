using System.ComponentModel.DataAnnotations;

namespace WabbaBot.Core.Interfaces {
    public interface IHasId {
        public int Id { get; set; }
    }
}
