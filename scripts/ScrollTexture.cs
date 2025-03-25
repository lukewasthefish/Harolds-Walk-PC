using Godot;
using System;

public partial class ScrollTexture : MeshInstance3D
{
	[Export]
	private Vector2 scrollRate;

	public override void _Process(double delta)
	{
		StandardMaterial3D standardMaterial3D = (StandardMaterial3D)this.GetSurfaceOverrideMaterial(0);
		standardMaterial3D.Uv1Offset = new Vector3(standardMaterial3D.Uv1Offset.X + scrollRate.X * (float)delta, standardMaterial3D.Uv1Offset.Y + scrollRate.Y * (float)delta, 0f);
	}
}
