using System;
using System.Threading;
using System.Threading.Tasks;

namespace BruteForceCracker
{
    public class BruteForceGenerator
    {
        // Executes a single-threaded sequential brute force attack
        public string RunSingleThreaded(PasswordValidator validator, Action<string, long> reportProgress, CancellationToken token)
        {
            long totalAttempts = 0;
            string alphabet = PasswordManager.ALPHABET;

            for (int length = 1; length <= 6; length++)
            {
                if (token.IsCancellationRequested) break;

                char[] currentGuess = new char[length];
                string result = GeneratePermutations(0, currentGuess, alphabet, validator, ref totalAttempts, reportProgress, token);
                if (result != null) return result;
            }
            return null;
        }

        // Executes a multi-threaded parallel brute force attack capped at (CPU Cores - 1)
        public string RunMultiThreaded(PasswordValidator validator, Action<string, long> reportProgress, CancellationToken token)
        {
            long totalAttempts = 0;
            string alphabet = PasswordManager.ALPHABET;
            string foundPassword = null;

            for (int length = 1; length <= 6; length++)
            {
                if (token.IsCancellationRequested) break;

                ParallelOptions options = new ParallelOptions
                {
                    // Limits thread utilization to maximum of CPU cores - 1
                    MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1),
                    CancellationToken = token
                };

                try
                {
                    // Demonstrates parallel execution by partitioning work across alphabet characters
                    Parallel.ForEach(alphabet, options, (firstChar, state) =>
                    {
                        char[] currentGuess = new char[length];
                        currentGuess[0] = firstChar;

                        if (length == 1)
                        {
                            long attempts = Interlocked.Increment(ref totalAttempts);
                            string guess = new string(currentGuess);
                            if (validator.IsMatch(guess))
                            {
                                foundPassword = guess;
                                state.Stop(); // Halts execution loops immediately
                            }
                            return;
                        }

                        string result = GenerateMultiThreadedPermutations(1, currentGuess, alphabet, validator, ref totalAttempts, reportProgress, token, state);
                        if (result != null)
                        {
                            foundPassword = result;
                            state.Stop(); // Halts execution loops immediately
                        }
                    });
                }
                catch (OperationCanceledException) { }

                if (foundPassword != null) return foundPassword;
            }
            return null;
        }

        private string GeneratePermutations(int position, char[] currentGuess, string alphabet, PasswordValidator validator, ref long totalAttempts, Action<string, long> reportProgress, CancellationToken token)
        {
            if (token.IsCancellationRequested) return null;

            if (position == currentGuess.Length)
            {
                totalAttempts++;
                string guess = new string(currentGuess);

                if (totalAttempts % 100000 == 0) reportProgress?.Invoke(guess, totalAttempts);

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

        // Recursively generates character combinations across multiple partitioned threads
        private string GenerateMultiThreadedPermutations(int position, char[] currentGuess, string alphabet, PasswordValidator validator, ref long totalAttempts, Action<string, long> reportProgress, CancellationToken token, ParallelLoopState state)
        {
            if (token.IsCancellationRequested || state.IsStopped) return null;

            if (position == currentGuess.Length)
            {
                long attempts = Interlocked.Increment(ref totalAttempts);
                string guess = new string(currentGuess);

                if (attempts % 500000 == 0) reportProgress?.Invoke(guess, attempts);

                if (validator.IsMatch(guess))
                {
                    reportProgress?.Invoke(guess, attempts);
                    return guess;
                }
                return null;
            }

            for (int i = 0; i < alphabet.Length; i++)
            {
                if (token.IsCancellationRequested || state.IsStopped) return null;

                currentGuess[position] = alphabet[i];
                string found = GenerateMultiThreadedPermutations(position + 1, currentGuess, alphabet, validator, ref totalAttempts, reportProgress, token, state);
                if (found != null) return found;
            }
            return null;
        }
    }
}
