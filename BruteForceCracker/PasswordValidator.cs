namespace BruteForceCracker
{
    public class PasswordValidator
    {
        private readonly string _targetHash;

        // Initializes validator with targeted hash
        public PasswordValidator(string targetHash)
        {
            _targetHash = targetHash;
        }

        // Compares hash of a plain text guess against target hash
        public bool IsMatch(string plainTextGuess)
        {
            string guessHash = PasswordManager.ComputeHash(plainTextGuess);
            return guessHash == _targetHash;
        }
    }
}