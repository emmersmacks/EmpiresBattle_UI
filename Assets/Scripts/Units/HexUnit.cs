using System;
using System.Collections;
using System.Collections.Generic;
using EmpiresBattle.Grid;
using UnityEngine;

namespace EmpiresBattle.Units
{
    /// <summary>
    /// A unit that lives on the hex grid, occupying one <see cref="HexCell"/> at a time.
    /// </summary>
    public class HexUnit : MonoBehaviour
    {
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
        private static readonly int FireHash = Animator.StringToHash("Fire");

        [SerializeField] private int moveRange = 3;

        [Tooltip("Cell this unit starts on. Assign it in the editor, then use \"Snap To Assigned Cell\" to preview the placement.")]
        [SerializeField] private HexCell startingCell;

        [Tooltip("World units per second while walking along a path.")]
        [SerializeField] private float moveSpeed = 2f;

        [Tooltip("Optional. Defaults to a child Animator (e.g. the sprite) if left empty.")]
        [SerializeField] private Animator animator;

        private Coroutine _moveCoroutine;
        private Coroutine _attackCoroutine;
        private float _baseScaleX;

        public int MoveRange => moveRange;
        public HexCell CurrentCell { get; private set; }
        public HexCoord CurrentCoord => CurrentCell.Coord;
        public bool IsMoving { get; private set; }
        public bool IsAttacking { get; private set; }
        public bool IsBusy => IsMoving || IsAttacking;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            _baseScaleX = Mathf.Abs(transform.localScale.x);
        }

        private void Start()
        {
            if (CurrentCell == null && startingCell != null)
            {
                PlaceOnCell(startingCell);
            }
        }

        /// <summary>Moves the unit's transform to the assigned starting cell without registering occupancy. Editor preview only.</summary>
        public void SnapToStartingCell()
        {
            if (startingCell != null)
            {
                transform.position = startingCell.transform.position;
            }
        }

        /// <summary>Initial placement onto the grid, with no previous cell to vacate.</summary>
        public void PlaceOnCell(HexCell cell)
        {
            CurrentCell = cell;
            cell.SetOccupant(this);
            transform.position = cell.transform.position;
        }

        /// <summary>Instantly moves the unit to another cell, updating occupancy on both cells.</summary>
        public void TeleportTo(HexCell targetCell)
        {
            if (CurrentCell != null)
            {
                CurrentCell.ClearOccupant();
            }

            transform.position = targetCell.transform.position;
            targetCell.SetOccupant(this);
            CurrentCell = targetCell;
        }

        /// <summary>Plays the attack animation on an adjacent target cell, blocking movement until it finishes.</summary>
        public void Attack(Action onComplete = null)
        {
            if (IsBusy)
            {
                onComplete?.Invoke();
                return;
            }

            _attackCoroutine = StartCoroutine(AttackRoutine(onComplete));
        }

        private IEnumerator AttackRoutine(Action onComplete)
        {
            IsAttacking = true;

            if (animator != null)
            {
                animator.SetTrigger(FireHash);
                yield return null;

                // The Fire state shares its name with the Fire trigger, so the same hash
                // identifies both; wait until the state machine has left it (after its
                // exit-time transition back to Idle/Run) before unlocking.
                while (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == FireHash)
                {
                    yield return null;
                }
            }

            IsAttacking = false;
            _attackCoroutine = null;
            onComplete?.Invoke();
        }

        /// <summary>Walks the unit along a path of cells, one hex at a time, then invokes <paramref name="onComplete"/>.</summary>
        public void MoveAlongPath(List<HexCell> path, Action onComplete = null)
        {
            if (IsBusy || path == null || path.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            _moveCoroutine = StartCoroutine(MoveRoutine(path, onComplete));
        }

        private IEnumerator MoveRoutine(List<HexCell> path, Action onComplete)
        {
            IsMoving = true;

            if (animator != null)
            {
                animator.SetBool(IsRunningHash, true);
            }

            if (CurrentCell != null)
            {
                CurrentCell.ClearOccupant();
            }

            HexCell arrivedCell = CurrentCell;

            foreach (HexCell nextCell in path)
            {
                Vector3 startPos = transform.position;
                Vector3 endPos = nextCell.transform.position;
                UpdateFacing(startPos, endPos);
                float distance = Vector3.Distance(startPos, endPos);
                float duration = moveSpeed > 0f ? distance / moveSpeed : 0f;
                float t = 0f;

                while (t < duration)
                {
                    t += Time.deltaTime;
                    transform.position = Vector3.Lerp(startPos, endPos, t / duration);
                    yield return null;
                }

                transform.position = endPos;
                arrivedCell = nextCell;
            }

            CurrentCell = arrivedCell;
            CurrentCell.SetOccupant(this);

            if (animator != null)
            {
                animator.SetBool(IsRunningHash, false);
            }

            IsMoving = false;
            _moveCoroutine = null;
            onComplete?.Invoke();
        }

        /// <summary>Flips the root transform's scale.x to face the direction of the current path leg.</summary>
        private void UpdateFacing(Vector3 fromPos, Vector3 toPos)
        {
            float dx = toPos.x - fromPos.x;

            if (Mathf.Abs(dx) > 0.0001f)
            {
                Vector3 scale = transform.localScale;
                scale.x = dx < 0f ? -_baseScaleX : _baseScaleX;
                transform.localScale = scale;
            }
        }
    }
}
