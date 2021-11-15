using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace BabiesAndChildren
{

	public class JobDriver_PlayAround : JobDriver
	{

		public int Ticks
		{
			get
			{
				return Find.TickManager.TicksGame - this.startTick;
			}
		}


		public override Vector3 ForcedBodyOffset
		{
			get
			{
				float num = Mathf.Sin((float)this.Ticks / 60f * 8f);
				float z = Mathf.Max(Mathf.Pow((num + 1f) * 0.5f, 2f) * 0.2f - 0.06f, 0f);
				return new Vector3(0f, 0f, z);

			}
		}


		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = new Toil();
			this.jumping = Rand.Bool;
			toil.tickAction = delegate ()
			{
				if (this.Ticks % 10 == 0)
				{
					this.jumping = !this.jumping;
				}
				if (this.Ticks % 120 == 0 && !this.jumping)
				{
					this.pawn.Rotation = Rot4.Random;
				}
				this.pawn.needs.joy.GainJoy(this.job.def.joyGainRate * 0.000644f, JoyKindDefOf.Social);
			};
			toil.socialMode = RandomSocialMode.SuperActive;
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 20;
			toil.handlingFacing = true;
			yield return toil;
			yield break;
		}


	private bool jumping;
	}
}
