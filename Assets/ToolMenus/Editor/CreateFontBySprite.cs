using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEditor;
using UnityEditor.U2D;

using UnityEngine;
using UnityEngine.U2D;

namespace UFrame
{
    public class CreateFontBySprite
    {
        [MenuItem("Assets/UFrame/Sprite2Font")]
        private static void CreateFont()
        {
            if (Selection.activeObject is Texture2D)
            {
                Texture2Font(Selection.activeObject as Texture2D);
            }
            else if (Selection.activeObject is SpriteAtlas)
            {
                Texture2Font(Selection.activeObject as SpriteAtlas);
            }
        }
        [MenuItem("Assets/UFrame/Sprite2Font",validate = true)]
        private static bool CreateFontCheck()
        {
            if (Selection.activeObject is Texture2D)
            {
                return true;
            }
            else if(Selection.activeObject is SpriteAtlas)
            {
                return true;
            }
            return false;
        }

        public static void Texture2Font(Texture2D tex)
        {
            string selectionPath = AssetDatabase.GetAssetPath(tex);
            var assets = AssetDatabase.LoadAllAssetsAtPath(selectionPath);
            List<Sprite> sprites = new List<Sprite>();
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite)
                {
                    sprites.Add(assets[i] as Sprite);
                }
            }
            string loadPath = Path.GetFileNameWithoutExtension(selectionPath);
            string fontPathName = loadPath + ".fontsettings";
            string matPathName = loadPath + ".mat";
            float lineSpace = 0.1f;
            if (sprites.Count > 0)
            {
                Material mat = new Material(Shader.Find("GUI/Text Shader"));
                ProjectWindowUtil.CreateAsset(mat, matPathName);
                mat.SetTexture("_MainTex", tex);
                Font font = new Font();
                font.material = mat;
                ProjectWindowUtil.CreateAsset(font, fontPathName);
                CharacterInfo[] characterInfo = new CharacterInfo[sprites.Count];

                for (int i = 0; i < sprites.Count; i++)
                {
                    var rect = sprites[i].rect;
                    if (rect.height > lineSpace)
                    {
                        lineSpace = rect.height;
                    }
                }

                for (int i = 0; i < sprites.Count; i++)
                {
                    Sprite spr = sprites[i];
                    CharacterInfo info = new CharacterInfo();
                    info.index = (int)spr.name[spr.name.Length - 1];
                    Rect rect = spr.rect;
                    float pivot = spr.pivot.y / rect.height - 0.5f;
                    if (pivot > 0)
                    {
                        pivot = -lineSpace / 2 - spr.pivot.y;
                    }
                    else if (pivot < 0)
                    {
                        pivot = -lineSpace / 2 + rect.height - spr.pivot.y;
                    }
                    else
                    {
                        pivot = -lineSpace / 2;
                    }
                    int offsetY = (int)(pivot + (lineSpace - rect.height) / 2);
                    //设置字符映射到材质上的坐标  
                    info.uvBottomLeft = new Vector2((float)rect.x / tex.width, (float)(rect.y) / tex.height);
                    info.uvBottomRight = new Vector2((float)(rect.x + rect.width) / tex.width, (float)(rect.y) / tex.height);
                    info.uvTopLeft = new Vector2((float)rect.x / tex.width, (float)(rect.y + rect.height) / tex.height);
                    info.uvTopRight = new Vector2((float)(rect.x + rect.width) / tex.width, (float)(rect.y + rect.height) / tex.height);
                    //设置字符顶点的偏移位置和宽高  
                    info.minX = 0;
                    info.minY = -(int)rect.height - offsetY;
                    info.maxX = (int)rect.width;
                    info.maxY = -offsetY;
                    //设置字符的宽度  
                    info.advance = (int)rect.width;
                    characterInfo[i] = info;
                }
                font.characterInfo = characterInfo;
                EditorUtility.SetDirty(font);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("Max Height：" + lineSpace + "  Prefect Height：" + (lineSpace + 2));
            }
        }

        public static void Texture2Font(SpriteAtlas atlas)
        {
            var sourcePath = AssetDatabase.GetAssetPath(atlas);
            Sprite[] sprites = new Sprite[atlas.spriteCount];
            var numSprites = atlas.GetSprites(sprites);
            for (int i = 0; i < numSprites; i++)
            {
                sprites[i].name = sprites[i].name.Replace("(Clone)","");
            }
            string loadPath = Path.GetFileNameWithoutExtension(sourcePath);
            string fontPathName = loadPath + ".fontsettings";
            string matPathName = loadPath + ".mat";
            float lineSpace = 0.1f;
            if (numSprites > 0)
            {
                var tex = SpriteAtlasToTexture(atlas);
                Material mat = new Material(Shader.Find("GUI/Text Shader"));
                ProjectWindowUtil.CreateAsset(mat, matPathName);
                mat.SetTexture("_MainTex", tex);
                Debug.LogError(tex.width + "*" + tex.height);
                Font font = new Font();
                font.material = mat;
                ProjectWindowUtil.CreateAsset(font, fontPathName);
                CharacterInfo[] characterInfo = new CharacterInfo[numSprites];
                for (int i = 0; i < numSprites; i++)
                {
                    var rect = sprites[i].textureRect;
                    if (rect.height > lineSpace)
                    {
                        lineSpace = rect.height;
                    }
                }
                for (int i = 0; i < numSprites; i++)
                {
                    Sprite spr = sprites[i];
                    CharacterInfo info = new CharacterInfo();
                    info.index = (int)spr.name[spr.name.Length - 1];
                    Rect rect = spr.textureRect;
                    Debug.LogError("rect is not right,can`t use!");
                    float pivot = spr.pivot.y / rect.height - 0.5f;
                    if (pivot > 0)
                    {
                        pivot = -lineSpace / 2 - spr.pivot.y;
                    }
                    else if (pivot < 0)
                    {
                        pivot = -lineSpace / 2 + rect.height - spr.pivot.y;
                    }
                    else
                    {
                        pivot = -lineSpace / 2;
                    }
                    int offsetY = (int)(pivot + (lineSpace - rect.height) / 2);
                    //设置字符映射到材质上的坐标  
                    info.uvBottomLeft = new Vector2((float)rect.x / tex.width, (float)(rect.y) / tex.height);
                    info.uvBottomRight = new Vector2((float)(rect.x + rect.width) / tex.width, (float)(rect.y) / tex.height);
                    info.uvTopLeft = new Vector2((float)rect.x / tex.width, (float)(rect.y + rect.height) / tex.height);
                    info.uvTopRight = new Vector2((float)(rect.x + rect.width) / tex.width, (float)(rect.y + rect.height) / tex.height);
                    //设置字符顶点的偏移位置和宽高  
                    info.minX = 0;
                    info.minY = -(int)rect.height - offsetY;
                    info.maxX = (int)rect.width;
                    info.maxY = -offsetY;
                    //设置字符的宽度  
                    info.advance = (int)rect.width;
                    characterInfo[i] = info;
                }
                font.characterInfo = characterInfo;
                EditorUtility.SetDirty(font);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("Max Height：" + lineSpace + "  Prefect Height：" + (lineSpace + 2));
            }
            else
            {
                Debug.LogError("sprite empty!");
            }
        }

        public static Texture SpriteAtlasToTexture(SpriteAtlas atlas)
        {
            SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { atlas }, EditorUserBuildSettings.activeBuildTarget);
            MethodInfo getPreviewTextureMI = typeof(SpriteAtlasExtensions).GetMethod("GetPreviewTextures", BindingFlags.Static | BindingFlags.NonPublic);
            Texture2D[] atlasTextures = (Texture2D[])getPreviewTextureMI.Invoke(null, new System.Object[] { atlas });
            return atlasTextures[0];
        }
    }
}