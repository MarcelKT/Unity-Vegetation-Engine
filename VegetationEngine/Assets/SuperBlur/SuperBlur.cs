using UnityEngine;

namespace SuperBlur
{

	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Effects/Super Blur", -1)]
	public class SuperBlur : SuperBlurBase {
        public MouseHandler mouse;
		
		void OnRenderImage (RenderTexture source, RenderTexture destination)  {
			if (blurMaterial == null || UIMaterial == null) return;

			int tw = source.width >> downsample;
			int th = source.height >> downsample;

            RenderTexture texture = RenderTexture.GetTemporary(tw, th, 0, source.format);

			Graphics.Blit(source, texture);

			if (renderMode == RenderMode.Screen)
			{
				Blur(texture, destination);
			}
			else if (renderMode == RenderMode.UI)
			{
				Blur(texture, texture);
				UIMaterial.SetTexture(Uniforms._BackgroundTexture, texture);
				Graphics.Blit(source, destination);
			}
			else if (renderMode == RenderMode.OnlyUI)
			{
				Blur(texture, texture);
				UIMaterial.SetTexture(Uniforms._BackgroundTexture, texture);
			}

			RenderTexture.ReleaseTemporary(texture);
		}
			
	}

}
