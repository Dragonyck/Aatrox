namespace EntityStates.Aatrox
{
    public class AatroxSwordStance : BaseState
    {
        public static float healthCost = 0.15f;

        //private AatroxController aatroxController;

        public override void OnEnter()
        {
            base.OnEnter();

            //this.aatroxController = base.transform.root.GetComponent<AatroxController>();
            //if (this.aatroxController) aatroxController.EnablePrice();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.StartAimMode(0.2f, false);

            if (inputBank)
            {
                if (!inputBank.skill2.down)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
            }
        }

        public override void OnExit()
        {
            //if (this.aatroxController) this.aatroxController.EnableThirst();

            base.OnExit();
        }

        /*public void RefundStock()
        {
            //the template for copy paste, awful format
            if (base.skillLocator && base.skillLocator.secondary.stock < base.skillLocator.secondary.maxStock) base.skillLocator.secondary.AddOneStock();
        }*/

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }

}
