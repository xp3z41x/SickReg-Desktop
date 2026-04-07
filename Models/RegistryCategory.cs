namespace SickReg.Desktop.Models;

public enum RegistryCategory
{
    ComClsid,
    FileAssociation,
    SharedDll,
    ShellExtension,
    StartupEntry,
    UninstallEntry,
    EmptyKey,
    MuiCache,
    AppPath,
    FontReference,
    HelpFile,
    FileType
}

public static class RegistryCategoryExtensions
{
    public static string GetDisplayName(this RegistryCategory category) => category switch
    {
        RegistryCategory.ComClsid => "COM / ActiveX / CLSID",
        RegistryCategory.FileAssociation => "Associacoes de Arquivo",
        RegistryCategory.SharedDll => "DLLs Compartilhadas",
        RegistryCategory.ShellExtension => "Extensoes de Shell",
        RegistryCategory.StartupEntry => "Entradas de Inicializacao",
        RegistryCategory.UninstallEntry => "Entradas de Desinstalacao",
        RegistryCategory.EmptyKey => "Chaves Vazias",
        RegistryCategory.MuiCache => "MUI Cache",
        RegistryCategory.AppPath => "Caminhos de Aplicativos",
        RegistryCategory.FontReference => "Referencias de Fontes",
        RegistryCategory.HelpFile => "Arquivos de Ajuda",
        RegistryCategory.FileType => "Tipos de Arquivo",
        _ => category.ToString()
    };
}
