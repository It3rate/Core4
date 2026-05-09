namespace Gojo.GearDemo;

public static class GearDemoHostServices
{
    public static Func<Task>? PublishWebAsync { get; set; }

    public static bool CanPublishWeb => PublishWebAsync is not null;
}
