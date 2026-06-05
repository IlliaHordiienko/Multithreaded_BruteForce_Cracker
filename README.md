# Multithreaded SHA-256 Brute Force Password Cracker

An asynchronous C# WPF application designed to demonstrate the performance differences between single-threaded and multi-threaded cryptographic brute-force attacks.

## Version History

### Stage 1: Project Initialization and Git Setup
- Initialized blank .NET WPF application template inside the local repository.
- Configured and verified Git tracking controls, .gitignore, and repository structure.

### Stage 2: Graphical User Interface Design
- Designed user interface layout with necessary progress controls and status displays.
- Added foundational event hooks in MainWindow.xaml.cs for execution control.

### Stage 3: Cryptography and Password Setup
- Created `PasswordManager` class for isolated logic.
- Implemented SHA-256 hashing with a constant static salt.
- Configured random password generator strictly bound to [4-6) character limits.

### Stage 4: Brute-Force Core Logic
- Developed independent `PasswordValidator` class to decouple validation from generation.
- Implemented recursive `BruteForceGenerator` covering character lengths from 1 to 6 sequentially.