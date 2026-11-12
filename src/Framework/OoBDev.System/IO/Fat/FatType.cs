namespace OoBDev.System.IO.Fat;

/// <summary>
/// The FAT variant of a volume, decided (as the specification requires) by the number of data clusters.
/// </summary>
public enum FatType
{
    /// <summary>Fewer than 4,085 clusters; 12-bit table entries.</summary>
    Fat12,

    /// <summary>4,085 to 65,524 clusters; 16-bit table entries.</summary>
    Fat16,

    /// <summary>65,525 clusters or more; 32-bit table entries (28 bits used) and a root directory in the cluster chain.</summary>
    Fat32,
}
