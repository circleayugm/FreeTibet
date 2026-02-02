using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class VerticalMover
{
	private List<float> targetYList;

	public VerticalMover(IEnumerable<float> checkpoints)
	{
		// ソートしておく（下から上へ）
		targetYList = checkpoints.OrderBy(y => y).ToList();
	}

	/// <summary>
	/// 移動後の目標Y座標を返す。もうない場合はnull。
	/// </summary>
	public float? GetNextTargetY(float currentY, float deltaY)
	{
		if (deltaY == 0f) return null;

		const float epsilon = 0.01f; // 誤差許容

		if (deltaY > 0f)
		{
			// 上方向：今の位置をわずかに超えたものだけを候補に
			return targetYList.FirstOrDefault(y => y > currentY + epsilon);
		}
		else
		{
			// 下方向：今の位置をわずかに下回るものだけを候補に
			return targetYList.LastOrDefault(y => y < currentY - epsilon);
		}
	}

}
