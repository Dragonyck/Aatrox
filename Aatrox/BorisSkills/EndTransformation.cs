using Aatrox;

namespace EntityStates.Boris
{
    public class BorisEndTransformation : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            if (base.characterBody.master)
            {
                base.characterBody.master.bodyPrefab = AatroxPlugin.aatroxPrefab;
                base.characterBody.master.Respawn(base.characterBody.transform.position, base.characterBody.transform.rotation);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }

}
