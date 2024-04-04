using System;
using LegacyApp.Interfaces;

namespace LegacyApp
{
    public class UserService
    {
        private IClientRepository _clientRepository = new ClientRepository();
        private IUserCreditService _userCreditService = new UserCreditService();

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            var user = new User
            {
                Client = _clientRepository.GetById(clientId),
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
            SetUserCredits(user, clientId);
            if (!CheckUserDataCorrectness(user)) return false;
            UserDataAccess.AddUser(user);
            return true;
        }

        private bool CheckUserDataCorrectness(User user)
        {
            var age = AgeCalculator(user.DateOfBirth);
            if (string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName)) return false;
            if (!user.EmailAddress.Contains("@") || !user.EmailAddress.Contains(".")) return false;
            if (age < 21) return false;
            if (user.HasCreditLimit && user.CreditLimit < 500) return false;
            
            return true;
        }

        private int AgeCalculator(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;
            return age;
        }

        private void SetUserCredits(User user,int clientId)
        {
            if (_clientRepository.GetById(clientId).Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else if (_clientRepository.GetById(clientId).Type == "ImportantClient")
            {   
                int creditLimit = _userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                creditLimit = creditLimit * 2;
                user.CreditLimit = creditLimit;
            }
            else
            {
                user.HasCreditLimit = true;
                int creditLimit = _userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                user.CreditLimit = creditLimit;
            }
        }
    }
}
