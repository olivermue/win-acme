﻿using PKISharp.WACS.Configuration;
using System.Threading.Tasks;

namespace PKISharp.WACS.Services
{
    public interface IArgumentsService
    {
        MainArguments MainArguments { get; }
        T GetArguments<T>() where T : new();
        bool Active();
        bool HasFilter();
        Task<string> TryGetArgument(string providedValue, IInputService input, string what, bool secret = false);
        Task<string> TryGetArgument(string providedValue, IInputService input, string[] what, bool secret = false);
        string TryGetRequiredArgument(string optionName, string providedValue);
        void ShowHelp();
        void ShowCommandLine();
    }
}