using UnityEngine;
using UnityEditor;
using System.Linq;

public class PolyCounter : MonoBehaviour
{
    // Bu script editörün üst menüsüne "Tools" diye bir seçenek ekler.
    [MenuItem("Tools/En Yüksek Polygonlu Objeleri Bul")]
    public static void CountPolys()
    {
        // Sahnedeki SkinnedMeshRenderer (Karakterler) ve MeshFilter (Statik objeler) bileþenlerini bulur
        var skinnedMeshes = FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None);
        var staticMeshes = FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);

        var list = new System.Collections.Generic.List<MeshInfo>();

        // Karakterleri listeye ekle
        foreach (var smr in skinnedMeshes)
        {
            if (smr.sharedMesh != null)
                list.Add(new MeshInfo { name = smr.name, vertexCount = smr.sharedMesh.vertexCount, obj = smr.gameObject });
        }

        // Statik objeleri listeye ekle
        foreach (var mf in staticMeshes)
        {
            if (mf.sharedMesh != null)
                list.Add(new MeshInfo { name = mf.name, vertexCount = mf.sharedMesh.vertexCount, obj = mf.gameObject });
        }

        // Vertex sayýsýna göre çoktan aza sýrala
        var sortedList = list.OrderByDescending(x => x.vertexCount).Take(10).ToList(); // Ýlk 10 taneyi al

        Debug.Log("--- EN YÜKSEK POLYGONLU 10 OBJE ---");
        foreach (var item in sortedList)
        {
            Debug.Log($"Obje: <color=yellow>{item.name}</color> | Vertex Sayýsý: <color=red>{item.vertexCount}</color>", item.obj);
        }
    }

    class MeshInfo
    {
        public string name;
        public int vertexCount;
        public GameObject obj;
    }
}