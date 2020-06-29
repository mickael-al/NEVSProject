﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace DitzelGames.FastIK
{
    abstract public class AArme : NetworkBehaviour
    {
        public virtual IEnumerator shoot() { yield return null; }
        public virtual IEnumerator reload() { yield return null; }
        public virtual void CmdSendTire(){}
    }
}
