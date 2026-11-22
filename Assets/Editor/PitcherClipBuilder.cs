#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Ferramenta de editor: cria todas as animações (clips) de uma sprite sheet do Pitcher de uma vez.
/// Este arquivo PRECISA ficar dentro de uma pasta chamada "Editor" (ex.: Assets/Editor).
///
/// Como usar: selecione o PNG da sprite sheet (já fatiado em 48 sprites) no Project e vá em
/// menu Starship Bakery > Criar animações da arma (sprite sheet selecionada).
///
/// Formato esperado da sheet (4 linhas x 12 colunas):
///   Linhas, de cima para baixo: Baixo, Esquerda, Direita, Cima.
///   Em cada linha: 3 frames de ANDAR, 4 de DORMIR (adormecendo), 5 de PARADO (piscando).
/// </summary>
public static class PitcherClipBuilder
{
    // Caminho do objeto que tem o SpriteRenderer, a partir do objeto que tem o Animator.
    // No seu Pitcher o sprite fica no filho "Visual". Se estiver no mesmo objeto do Animator, deixe "".
    private const string RendererPath = "Visual";

    private static readonly string[] Directions = { "Baixo", "Esquerda", "Direita", "Cima" };
    private static readonly string[] GroupNames = { "Andar", "Dormir", "Parado" };
    private static readonly int[] GroupSizes = { 3, 4, 5 };
    private static readonly float[] GroupFps = { 8f, 4f, 4f };
    private static readonly bool[] GroupLoop = { true, false, true };   // Dormir termina e fica no último frame

    private const int Rows = 4;
    private const int PerRow = 12;

    [MenuItem("Starship Bakery/Criar animações da arma (sprite sheet selecionada)")]
    private static void Build()
    {
        Texture2D texture = Selection.activeObject as Texture2D;
        if (texture == null)
        {
            EditorUtility.DisplayDialog("Starship Bakery", "Selecione o PNG da sprite sheet no Project primeiro.", "OK");
            return;
        }

        string sheetPath = AssetDatabase.GetAssetPath(texture);
        List<Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToList();

        if (sprites.Count != Rows * PerRow)
        {
            EditorUtility.DisplayDialog("Starship Bakery",
                "Esperava " + (Rows * PerRow) + " sprites (4 linhas x 12 colunas) e achei " + sprites.Count + ".\n" +
                "Refaça o corte no Sprite Editor (Grid By Cell Size) e tente de novo.", "OK");
            return;
        }

        // Linhas de cima para baixo; dentro de cada linha, da esquerda para a direita.
        List<Sprite> byHeight = sprites.OrderByDescending(s => s.rect.center.y).ToList();
        List<List<Sprite>> rows = new List<List<Sprite>>();
        for (int r = 0; r < Rows; r++)
        {
            rows.Add(byHeight.Skip(r * PerRow).Take(PerRow).OrderBy(s => s.rect.center.x).ToList());
        }

        string folder = Path.GetDirectoryName(sheetPath).Replace('\\', '/') + "/Clips_" + texture.name;
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder(Path.GetDirectoryName(sheetPath).Replace('\\', '/'), "Clips_" + texture.name);

        int created = 0;
        for (int d = 0; d < Rows; d++)
        {
            int start = 0;
            for (int g = 0; g < GroupSizes.Length; g++)
            {
                List<Sprite> frames = rows[d].Skip(start).Take(GroupSizes[g]).ToList();
                start += GroupSizes[g];

                string clipName = texture.name + "_" + GroupNames[g] + "_" + Directions[d];
                CreateClip(frames, GroupFps[g], GroupLoop[g], folder + "/" + clipName + ".anim");
                created++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(folder);
        EditorUtility.DisplayDialog("Starship Bakery", created + " animações criadas em:\n" + folder, "OK");
    }

    private static void CreateClip(List<Sprite> frames, float fps, bool loop, string assetPath)
    {
        AnimationClip clip = new AnimationClip { frameRate = fps };

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = RendererPath,
            propertyName = "m_Sprite"
        };

        // Um frame extra repetindo o último, para todos os frames durarem o mesmo tempo.
        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[frames.Count + 1];
        for (int i = 0; i < frames.Count; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        keys[frames.Count] = new ObjectReferenceKeyframe { time = frames.Count / fps, value = frames[frames.Count - 1] };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        AssetDatabase.CreateAsset(clip, uniquePath);
    }
}
#endif
