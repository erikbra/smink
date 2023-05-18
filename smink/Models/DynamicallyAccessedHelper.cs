// using System.Diagnostics.CodeAnalysis;
// using smink.Models.XUnit;
//
// namespace smink.Models;
//
// public class DynamicallyAccessedHelper
// {
//     [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
//     static Type assemblies;
//     
//     [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
//     static Type assembly;
//     
//     public static void InitializeTypeField()
//     {
//         var asm = new Assemblies();
//         assemblies = typeof(Assemblies);
//         assembly = typeof(Assembly);
//     }
//     
// }