namespace backend.Models.Controllers
{
    internal class LoginResponse
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public UserData UserData { get; set; }
    }
}