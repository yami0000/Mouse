using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleBoundaryWrap : MonoBehaviour
{
    [Tooltip("设置为你想要的 3D 边界大小，应与 Shape 模块中的 Scale 保持一致")]
    public Vector3 boxSize = new Vector3(10, 10, 10);

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        int maxParticles = ps.main.maxParticles;
        if (particles == null || particles.Length < maxParticles)
        {
            particles = new ParticleSystem.Particle[maxParticles];
        }

        int aliveCount = ps.GetParticles(particles);
        Vector3 halfSize = boxSize / 2f;

        // 自动适配 Local 或 World 坐标系，防止坐标错乱形成隐形墙
        bool isWorldSpace = ps.main.simulationSpace == ParticleSystemSimulationSpace.World;
        Vector3 center = isWorldSpace ? transform.position : Vector3.zero;

        bool needsUpdate = false;

        for (int i = 0; i < aliveCount; i++)
        {
            Vector3 pos = particles[i].position;
            // 将坐标转换为相对于粒子系统中心的局部坐标
            Vector3 relativePos = pos - center;
            Vector3 oldPos = relativePos;

            // 使用 Mathf.Repeat 进行完美的数学空间折叠。
            // 它可以完美保留粒子超出边界的“余量距离”，从而彻底消灭“黏墙”现象。
            relativePos.x = Mathf.Repeat(relativePos.x + halfSize.x, boxSize.x) - halfSize.x;
            relativePos.y = Mathf.Repeat(relativePos.y + halfSize.y, boxSize.y) - halfSize.y;
            relativePos.z = Mathf.Repeat(relativePos.z + halfSize.z, boxSize.z) - halfSize.z;

            // 只有当粒子发生越界传送时（坐标变化幅度极大），才重新写入数据以节省性能
            if (Mathf.Abs(oldPos.x - relativePos.x) > 1f ||
                Mathf.Abs(oldPos.y - relativePos.y) > 1f ||
                Mathf.Abs(oldPos.z - relativePos.z) > 1f)
            {
                particles[i].position = relativePos + center;
                needsUpdate = true;
            }
        }

        if (needsUpdate)
        {
            ps.SetParticles(particles, aliveCount);
        }
    }
}