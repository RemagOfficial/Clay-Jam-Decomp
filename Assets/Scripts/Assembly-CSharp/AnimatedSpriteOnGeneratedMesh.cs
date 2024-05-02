using UnityEngine;

[RequireComponent(typeof(AnimatedSprite))]
public class AnimatedSpriteOnGeneratedMesh : MonoBehaviour
{
	public Material _Material;

	private Mesh _mesh;

	private AnimatedSprite _animatedSprite;

	private void Awake()
	{
		MeshFilter meshFilter = base.gameObject.AddComponent<MeshFilter>();
		_mesh = new Mesh();
		MeshCreation.CreateSingleQuad(_mesh, base.transform.localScale.x, base.transform.localScale.y);
		meshFilter.sharedMesh = _mesh;
		base.gameObject.AddComponent<MeshRenderer>();
		base.renderer.material = _Material;
		_animatedSprite = GetComponent<AnimatedSprite>();
	}

	private void Start()
	{
		UpdateFromAnimatedSprite();
	}

	private void Update()
	{
		UpdateFromAnimatedSprite();
	}

	private void UpdateFromAnimatedSprite()
	{
		Vector2 offset = _animatedSprite.CurrentFrameData.Offset;
		offset.y -= _animatedSprite.CurrentFrameData.Scale.y;
		base.renderer.material.mainTexture = _animatedSprite.CurrentFrameData.Texture;
		base.renderer.material.mainTextureOffset = offset;
		base.renderer.material.mainTextureScale = _animatedSprite.CurrentFrameData.Scale;
	}
}
