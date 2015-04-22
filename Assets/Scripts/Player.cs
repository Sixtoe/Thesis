﻿using UnityEngine;
using System.Collections;

public static class Controls { 
	public static int Left = 1 << 1,
		Right = 1 << 2, 
		Jump = 1 << 3, 
		Shoot = 1 << 4;
}

[RequireComponent(typeof(MotionComponent))]
public class Player : StepBasedComponent {

	public float Gravity;
	public float MoveSpeed;
	public float JumpVelocity;
	public float BoostReduction;
	public int Health = 100;
	public GameObject BulletPrefab;

	protected int Movement;

	protected MotionComponent motion;
	protected CollisionComponent coll;

	protected bool onGround;

	protected bool facingRight;

	virtual public void Awake() {
		motion = GetComponent<MotionComponent> ();
		coll = GetComponent<CollisionComponent> ();
	}

	override public void OnEnable() {
		base.OnEnable ();
		motion.OnWallHit += WallHit;
	}

	override public void OnDisable() {
		base.OnDisable ();
		motion.OnWallHit -= WallHit;
	}

	protected void HandleInput(int input) {
		Movement = Movement | input;
	}

	override public void Step() {
		bool grounded = (Level.current.SolidAtPoint (new Vector2 (transform.position.x, transform.position.y - coll.extents.y - .1f)));

		if (!grounded && onGround) {
			onGround = false;
			LeftGround();
		}
		else if (!onGround && grounded) {
			onGround = true;
			Landed();
		}

		float mx = 0f, my = motion.Velocity.y; //motion.Velocity.x;
		if ((Movement & Controls.Left) > 0) {
			mx -= MoveSpeed;
			facingRight = false;
		} else if ((Movement & Controls.Right) > 0) {
			mx += MoveSpeed;
			facingRight = true;
		}

		if ((Movement & Controls.Jump) > 0) { 
			if (grounded)
				my = JumpVelocity;
		} else if (my > 0f)
			my = Mathf.Max (0f, my - BoostReduction);

		motion.Velocity = new Vector2 (mx, my + Gravity);
		Movement = 0;
	}


	virtual public void WallHit(Direction contactDir) {
		if (contactDir == Direction.Up || contactDir == Direction.Down) {
			motion.Velocity = new Vector2 (motion.Velocity.x, 0f);
		}
	}

	virtual public void HandleShoot(Vector2 velocity) {
		GameObject bullet = Instantiate<GameObject> (BulletPrefab);
		bullet.GetComponent<MotionComponent> ().Velocity = velocity;
		bullet.GetComponent<CollisionComponent> ().Start ();
		bullet.transform.position = transform.position + (Vector3) (3f * velocity);
	}

	virtual public void Landed() {}
	virtual public void LeftGround() {}
}
