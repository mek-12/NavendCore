# Yerel (Local) NuGet Kaynağı Tanımlama ve Paket Ekleme

Yeni bir sürüme geçmek ve güncel bir NuGet paketi oluşturmak istiyorsanız, `.sln` dosyasındaki sürüm numarasını artırıp aşağıdaki komutu çalıştırabilirsiniz:

```bash
dotnet build -c Release
```

Eğer daha önce yerel NuGet kaynağı tanımlamadıysanız, önce `nuget-local` adında bir kaynak oluşturalım:

```bash
dotnet nuget add source ~/.nuget/nuget-local --name Local
```

Sonrasında oluşturduğunuz NuGet paketini bu kaynağa kopyalayın:

```bash
cp ./Navend.Core/bin/Release/*.nupkg ~/.nuget/nuget-local/
```

Artık `Navend.Core` paketinin bu yeni sürümünü, projelerinize yerel bir kaynak olarak ekleyip kullanabilirsiniz.
