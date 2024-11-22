namespace EvYoneticisiAPI.Models
{
    public class UserExpense
    {
        public int UserExpenseId { get; set; }
        public int UserId { get; set; }
        public int ExpenseId { get; set; }

        public User User { get; set; }
        public Expense Expense { get; set; }
    }
}
