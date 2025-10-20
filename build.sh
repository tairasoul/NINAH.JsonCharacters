dotnet build -p:Configuration=Release
rm ../ninah-gamedir/BepInEx/plugins/JsonCharacters.dll
rm ../ninah-gamedir/BepInEx/plugins/JsonCharacters.pdb
cp bin/Release/net6.0/JsonCharacters.dll ../ninah-gamedir/BepInEx/plugins/
cp bin/Release/net6.0/JsonCharacters.pdb ../ninah-gamedir/BepInEx/plugins/
