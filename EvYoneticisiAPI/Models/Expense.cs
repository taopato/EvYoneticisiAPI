namespace EvYoneticisiAPI.Models
{
    public class Expense
    {
        public int ExpenseId { get; set; }
        public string ItemName { get; set; }
        public decimal Cost { get; set; }
        public bool IsShared { get; set; }
        public bool IsApproved { get; set; }
        public ICollection<UserExpense> UserExpenses { get; set; }
    }
}
