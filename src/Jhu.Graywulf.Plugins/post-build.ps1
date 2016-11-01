﻿param(
	[string]$SolutionDir,
	[string]$SolutionName,
	[string]$ProjectDir,
	[string]$ProjectName,
	[string]$OutDir,
	[string]$TargetName
)

cp $ProjectDir$OutDir$TargetName.dll $SolutionDir$OutDir
cp $ProjectDir$OutDir$TargetName.pdb $SolutionDir$OutDir

# copy to test

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\test\Jhu.Graywulf.Jobs.Test\${OutDir}
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\test\Jhu.Graywulf.Jobs.Test\${OutDir}

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\test\Jhu.Graywulf.Scheduler.Test\${OutDir}
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\test\Jhu.Graywulf.Scheduler.Test\${OutDir}
