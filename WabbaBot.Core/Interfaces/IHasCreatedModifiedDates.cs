namespace WabbaBot.Interfaces {
    public interface IHasCreatedModifiedDates {
        public DateTime CreatedOn { get; }
        public DateTime ModifiedOn { get; set; }
    }
}
