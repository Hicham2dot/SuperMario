# SuperMario — Contexte projet (référence rapide)

> Ce fichier évite de relire tous les scripts à chaque session.
> Mettre à jour ici quand un script change de logique ou d'interface.

---

## Vue d'ensemble

Prototype Unity 3D — **survie + collecte** sur un plan (Plane).
- Joueur : se déplace en 3D, peut sauter, peut tomber du bord → Game Over
- Ennemis : bougent aléatoirement, poussent le joueur par collision physique (pas de dégât direct)
- Pièces : à collecter pour augmenter le score, respawn aléatoire
- Seule condition de défaite : tomber dans la DeathZone sous le sol

---

## Architecture des scripts

Tous les scripts sont dans `Assets/Scripts/`.

### PlayerController.cs
**Rôle :** Mouvement 3D + saut du joueur.  
**Requiert :** `Rigidbody` sur le même GameObject.  
**Inputs :** WASD / flèches pour X/Z, `Space` pour sauter.  
**Clés :**
- `maxSpeed`, `acceleration`, `deceleration` — vitesse de déplacement
- `jumpForce`, `fallMultiplier`, `maxFallSpeed` — physique du saut
- `groundCheckRadius`, `groundLayer` — détection sol via `Physics.CheckSphere`
- Coyote time (0.12 s) + jump buffer (0.12 s) intégrés
- Input (inputX, inputZ, jumpRequested) lu dans `Update`, physique (ApplyMovement, ApplyJump, ApplyFallGravity) dans `FixedUpdate`
- Le joueur s'oriente automatiquement via `Quaternion.LookRotation(inputDir)`
- `RigidbodyConstraints` : freeze rotation X et Z (peut tourner en Y seulement)
- `groundCheck` créé dynamiquement à `localPosition (0, -0.9, 0)` si non assigné

### EnemyController.cs
**Rôle :** Ennemi lourd à déplacement aléatoire qui pousse le joueur physiquement, reste sur le sol.  
**Requiert :** `Rigidbody` + Collider **non-trigger** (`[RequireComponent(typeof(Rigidbody))]`).  
**Clés :**
- `moveSpeed` (défaut 3) — vitesse de déplacement
- `changeDirectionInterval` (défaut 2 s) — fréquence du changement de direction
- `mass` (défaut 8) — masse définie dans Start via `rb.mass` (player = 1 par défaut → ennemi 8× plus lourd)
- `groundHalfSize` (défaut 4.5) — demi-taille du Plane (Plane Unity 10×10 → half=5, marge 0.5)
- `FreezePositionY` activé → l'ennemi ne quitte JAMAIS le sol (pas de saut accidentel)
- `CheckBounds()` appelé dans FixedUpdate : réfléchit la direction si le bord est atteint + clamp position
- Direction choisie via angle aléatoire → `Mathf.Cos/Sin` → vecteur normalisé X/Z
- `OnCollisionEnter` : change de direction à chaque collision (joueur ou autre ennemi)
- **Ne réduit jamais le score ni les vies — pousse uniquement par physique**
- `pos.y = 0.5f` dans Start : à adapter selon la hauteur du collider de l'ennemi

### CoinCollectible.cs
**Rôle :** Pièce collectable — score + respawn.  
**Requiert :** Collider en mode **Trigger**.  
**Clés :**
- `value` (défaut 10) — points ajoutés au score
- `rotateSpeed` (défaut 90°/s) — rotation cosmétique en Y
- `respawnOnCollect` (défaut true) — si true : respawn aléatoire ; si false : Destroy
- `spawnAreaSize` (défaut 8) — demi-côté du carré de spawn centré sur (0,0)
- `spawnHeight` (défaut 0.5) — hauteur Y après respawn
- Détection via `OnTriggerEnter` — tag `"Player"` requis sur le joueur

### DeathZone.cs
**Rôle :** Zone invisible sous le sol → Game Over si le joueur y tombe.  
**Requiert :** Collider en mode **Trigger**, plus grand que le Plane.  
**Clés :**
- `OnTriggerEnter` → appelle `GameManager.Instance.GameOver()`
- Aucun autre comportement

### GameManager.cs
**Rôle :** Singleton central — score, Game Over, redémarrage.  
**Clés :**
- `GameManager.Instance` — accès global depuis tous les scripts
- `scoreText` (TMP_Text) — UI score à assigner dans l'Inspector
- `gameOverText` (TMP_Text) — affiché au Game Over, caché au démarrage
- `restartDelay` (défaut 3 s) — délai avant rechargement de scène
- `AddScore(int points)` — incrémente + met à jour l'UI (ignoré si Game Over)
- `GameOver()` — public, désactive le joueur, affiche le texte, lance le coroutine restart
- `isGameOver` — guard interne pour éviter les doubles déclenchements
- **Pas de système de vies** — une seule chute = Game Over immédiat

### CameraController.cs
**Rôle :** Suit le joueur en 3D avec un offset fixe.  
**Clés :**
- `target` (Transform) — à assigner sur le joueur dans l'Inspector
- `offsetX/Y/Z` — défauts : 0 / 10 / -7 (vue quart-haut derrière le joueur)
- `smoothSpeed` (défaut 6) — réactivité du suivi via `Vector3.Lerp`
- `LateUpdate` : `transform.LookAt(target)` — la caméra regarde toujours le joueur
- Pas de contraintes de bords (pas de style Mario latéral)

---

## Tags Unity requis

| Tag | Utilisé par |
|-----|-------------|
| `Player` | EnemyController, CoinCollectible, DeathZone, GameManager |

---

## Setup scène (checklist)

- [ ] **Joueur** : GameObject avec `Rigidbody` + Collider + tag `Player` + `PlayerController`
- [ ] **Ennemis** : GameObject avec `Rigidbody` + Collider (non-trigger) + `EnemyController`
- [ ] **Pièce** : GameObject avec Collider en **Trigger** + `CoinCollectible`
- [ ] **DeathZone** : Plane invisible sous le sol, Collider en **Trigger**, taille > Plane, + `DeathZone`
- [ ] **GameManager** : GameObject vide avec `GameManager`, références UI assignées
- [ ] **Caméra** : `CameraController` avec `target` pointant sur le joueur

---

## Règles de gameplay (ne pas modifier sans mettre à jour ce fichier)

1. Les ennemis **ne tuent jamais directement** le joueur — ils poussent uniquement par physique
2. Le **seul Game Over** = chute dans la DeathZone
3. La pièce **respawn** par défaut (pas de destroy définitif)
4. Pas de système de vies — 1 chute = fin immédiate
5. Input classique Unity (`Input.GetKey`) — pas d'Input System avancé

---

## Dépendances

- **TextMeshPro** (TMPro) : requis par `GameManager` pour l'UI
- **UnityEngine.SceneManagement** : requis par `GameManager` pour le restart
- Unity version : 3D, physique standard (`Rigidbody`, `Physics.CheckSphere`)
- `rb.velocity` utilisé (compatible toutes versions Unity — pas `rb.linearVelocity` qui est Unity 6+ seulement)
- Input lu dans `Update`, stocké dans des champs privés, appliqué dans `FixedUpdate` (pattern obligatoire)
