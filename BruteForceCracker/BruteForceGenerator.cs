using System;
using System.Threading;

namespace BruteForceCracker
{
    public class BruteForceGenerator
    {
        // Executes a single-threaded sequential brute force attack
        public string RunSingleThreaded(PasswordValidator validator, Action<string, long> reportProgress, CancellationToken token)
        {
            long totalAttempts = 0;
            string alphabet = PasswordManager.ALPHABET;

            // Iterates from length 1 up to maximum length of 6
            for (int length = 1; length <= 6; length++)
            {
                if (token.IsCancellationRequested) break;

                char[] currentGuess = new char[length];
                string result = GeneratePermutations(0, currentGuess, alphabet, validator, ref totalAttempts, reportProgress, token);

                if (result != null) return result;
            }
            return null;
        }

        // Recursively generates character combinations for a designated length
        private string GeneratePermutations(int position, char[] currentGuess, string alphabet, PasswordValidator validator, ref long totalAttempts, Action<string, long> reportProgress, CancellationToken token)
        {
            if (token.IsCancellationRequested) return null;

            if (position == currentGuess.Length)
            {
                totalAttempts++;
                string guess = new string(currentGuess);

                // Triggers progress updates periodically to minimize UI overhead
                if (totalAttempts % 100000 == 0)
                {
                    reportProgress?.Invoke(guess, totalAttempts);
                }

                if (validator.IsMatch(guess))
                {
                    reportProgress?.Invoke(guess, totalAttempts);
                    return guess;
                }
                return null;
            }

            for (int i = 0; i < alphabet.Length; i++)
            {
                currentGuess[position] = alphabet[i];
                string found = GeneratePermutations(position + 1, currentGuess, alphabet, validator, ref totalAttempts, reportProgress, token);
                if (found != null) return found;
            }
            return null;
        }
    }
}
