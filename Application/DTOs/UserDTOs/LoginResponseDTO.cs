namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public class LoginResponseDTO
    {
        public int id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string role { get; set; }

        public LoginResponseDTO(int Id,string UserName, string Email, string Role)
        {
            id = Id;
            username = UserName;
            email = Email;
            role = Role;
        }

    }
    public class LoginTransDTO
    {
        public int  id { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public DateTime? refreshTokenExpiry { get; set; }

        public LoginTransDTO(int  Id, string UserName, string Email, string Role, string AccessToken, string RefreshToken,
            DateTime? RefreshTokenExpiry)
        {
            id = Id;
            userName = UserName;
            email = Email;
            role = Role;
            accessToken = AccessToken;
            refreshToken = RefreshToken;
            refreshTokenExpiry = RefreshTokenExpiry;
        }
    }
    
}
