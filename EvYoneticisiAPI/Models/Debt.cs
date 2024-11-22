namespace EvYoneticisiAPI.Models
{
    public class Debt
    {
        public int Id { get; set; }
        public int DebtorId { get; set; }
        public int CreditorId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public bool IsPaid { get; set; }
        public User Debtor { get; set; }
        public User Creditor { get; set; }
    }
}
