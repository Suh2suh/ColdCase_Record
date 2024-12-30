using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoEvidenceInfo : MonoBehaviour
{
	[Header("This will be matched with photo type in photo evidence board")]
	[SerializeField] Evidence evidenceType;
	public Evidence EvidenceType { get => evidenceType; }
}
