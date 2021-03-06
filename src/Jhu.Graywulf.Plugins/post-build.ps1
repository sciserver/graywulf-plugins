﻿$source = "$ProjectDir$OutDir"
$target1 = "$SolutionDir$OutDir"
$target2 = "$SolutionDir\plugins\$OutDir"

$files = @(
	"$TargetName.dll", 
	"$TargetName.pdb", 
	"DotNetOpenAuth.Core.dll", 
	"DotNetOpenAuth.OpenId.dll", 
	"DotNetOpenAuth.OpenId.RelyingParty.dll", 
	"ICSharpCode.SharpZipLib.dll", 
	"Mono.Math.dll", 
	"MySql.Data.dll", 
	"Newtonsoft.Json.dll", 
	"Npgsql.dll", 
	"Org.Mentalis.Security.Cryptography.dll", 
	"RabbitMQ.Client.dll", 
	"SciServer.Logging.dll", 
	"SciServer.Logging.pdb",
	"log4net.dll"
)

foreach ($f in $files) {
	cp "$source$f" "$target1"
	cp "$source$f" "$target2"
}
