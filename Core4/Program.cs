using Core4.Examples;

Console.WriteLine("Site settlement examples");
foreach (var line in SiteExamples.Describe())
{
    Console.WriteLine(line);
}

Console.WriteLine();
Console.WriteLine("Atomic basis examples");
foreach (var line in GradeOneExamples.DescribeAtomic())
{
    Console.WriteLine(line);
}

Console.WriteLine();
Console.WriteLine("Grade 1 examples (origin = 0)");
foreach (var line in GradeOneExamples.Describe())
{
    Console.WriteLine(line);
}

Console.WriteLine();
Console.WriteLine("Grade 1 examples (shifted origins)");
foreach (var line in GradeOneExamples.DescribeShiftedOrigins())
{
    Console.WriteLine(line);
}

Console.WriteLine();
Console.WriteLine("Atomic attached values");
foreach (var line in AttachmentExamples.DescribeAtomic())
{
    Console.WriteLine(line);
}

Console.WriteLine();
Console.WriteLine("Attached values (origin = 0)");
foreach (var line in AttachmentExamples.Describe())
{
    Console.WriteLine(line);
}

Console.WriteLine();
Console.WriteLine("Attached values (shifted origins)");
foreach (var line in AttachmentExamples.DescribeShiftedOrigins())
{
    Console.WriteLine(line);
}
