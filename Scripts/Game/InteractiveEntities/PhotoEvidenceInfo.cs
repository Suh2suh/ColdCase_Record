using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoEvidenceInfo : MonoBehaviour
{
	[Header("This will be matched with photo type in <b>photo evidence board</b>")]
	[SerializeField] Evidence evidenceType;
	public Evidence EvidenceType { get => evidenceType; }
}
