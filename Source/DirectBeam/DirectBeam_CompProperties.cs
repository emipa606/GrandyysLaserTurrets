using Verse;

namespace DirectBeam;

public class DirectBeam_CompProperties : CompProperties
{
    public readonly ThingDef lanceMoteDef = null;
    public float dbStartOffset = 0f;

    public DirectBeam_CompProperties()
    {
        compClass = typeof(CompDirectBeam);
    }
}