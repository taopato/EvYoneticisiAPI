namespace EvYoneticisiAPI.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public ICollection<UserExpense> UserExpenses { get; set; }
    }
}
