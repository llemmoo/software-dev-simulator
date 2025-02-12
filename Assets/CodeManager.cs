using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Unity.VisualScripting;

public class CodeManager : MonoBehaviour
{
    public static CodeManager Instance;
    private List<DragDropBlock> _blocks;
    private string _codeToExecute;
    
    private void Awake()
    {
        Instance = this;
        _blocks = new List<DragDropBlock>();
    }
    
    
    //Method to be invoked by the "run" button
    public void RunCode()
    {
        GenerateCodeFromBlocks(_blocks);
        
        if (!ValidateCode(_codeToExecute))
        {
            Debug.LogError("Invalid code");
        }
        else
        {
            try
            {
                ExecuteCode(_codeToExecute);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
    
    private void GenerateCodeFromBlocks(List<DragDropBlock> blocks)
    {
        string generatedCode = string.Empty;
        Debug.Log("Generating code from blocks");
        foreach (DragDropBlock block in blocks)
        {
            var blockBehaviour = block.GetComponent<BlockBehaviour>();
            generatedCode+=(blockBehaviour._blockInputFieldText);
        }
        Debug.Log(generatedCode);
        
        _codeToExecute = $@"
                using System;

                public class DynamicClass
                {{
                    public static void Execute()
                    {{
                {generatedCode}
                    }}
                }}";
    }
    
    static void ExecuteCode(string code)
    {
        // Parse the C# code
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        // Get references to required assemblies
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        // Create a Roslyn compilation
        var compilation = CSharpCompilation.Create(
            "DynamicAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary) // Create a DLL
        );

        // Compile the code into memory
        using (var ms = new MemoryStream())
        {
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                // Print any compilation errors
                var errors = result.Diagnostics
                    .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                    .Select(diagnostic => diagnostic.GetMessage());
                Debug.LogError("Compilation errors:");
                Debug.LogError(string.Join("\n", errors));
                return;
            }

            // Load the compiled assembly
            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());

            // Find and invoke the Execute method
            var type = assembly.GetType("DynamicClass");
            var method = type?.GetMethod("Execute");
            method?.Invoke(null, null);
        }
    }
    
    //Validation in order to ensure that we are trying to run code that can be compiled
    //Do we want to call this continuously everytime we see a new block or a new config?
    static bool ValidateCode(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();

        // Create a compilation
        var compilation = CSharpCompilation.Create(
            "Validation",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // Get diagnostics from the compilation
        var diagnostics = compilation.GetDiagnostics();
        foreach (var diagnostic in diagnostics)
        {
            Debug.LogError($"Error: {diagnostic.GetMessage()}");
        }

        // Return true if there are no errors
        return !diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
    }

    public void AddBlock(DragDropBlock block)
    {
        _blocks.Add(block);
    }
    public void DeleteBlock(DragDropBlock block)
    {
        _blocks.Remove(block);
    }
}
