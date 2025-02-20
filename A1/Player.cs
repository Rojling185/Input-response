using Godot;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;

public partial class Player : CharacterBody3D
{
    
    [Export]
    public float Speed { get; set; } = 10;
   

    [Export]
    public int FallAcceleration { get; set; } = 75;

    [Export]
    public int JumpImpulse { get; set; } = 20;

    [Export]
    Node3D NodeThing;

    [Export]
    public NodePath collisionShapePath;

    [Export]
    Curve Movement;
    [Export]
    Curve easein;

    [Export]
    public float airTime { get; set; } = 10;
    
    private Vector3 collisionPos;

    private CollisionShape3D CollisionShape;
    private Vector3 _targetVelocity = Vector3.Zero;
    int timer = 0;
    float timer2 = 1;
    float MoveTimer = 0;
    Vector3 axis = new Vector3(1, 1, 1);
    float rotationAmount = 0.1f;
    int AirTimer = 0;
    float air = 0;
    int axisTimer = 0;

    bool jumped = false;
    bool released = false;
    bool moving = false;

    int mode = 1;
    float jumpVel;
    float dirVel;
    Vector3 latestInput = Vector3.Zero;
    

    public override void _PhysicsProcess(double delta)
    {

        if(Input.IsActionPressed("1") || mode ==1 ){
            mode =1;
            dirVel = Movement.Sample(MoveTimer);
        }
        if(Input.IsActionPressed("2") || mode ==2 ){
            mode =2;
            dirVel = Movement.Sample(MoveTimer);
        }
        if(Input.IsActionPressed("3") || mode ==3 ){
            mode = 3;
            if(moving){
                dirVel = easein.Sample(MoveTimer);
            }
            //dirVel = easein.Sample(MoveTimer);
        }
        CollisionShape = GetNode<CollisionShape3D>(collisionShapePath);
        collisionPos = CollisionShape.Position;
        
        bool crouch = false;
        var direction = Vector3.Zero;
        

        if(IsOnFloor()){
            timer = 0;
            
            NodeThing.Scale = new Vector3(1,1,1);
            _targetVelocity.Y = 0;
            if(!jumped){
                timer2 = 1;
                
                
            }
            
            //GD.Print("floor");
        }

        if (Input.IsActionPressed("move_right"))
        {
            direction.X += dirVel;
            moving = true;
        }
        if (Input.IsActionPressed("move_left"))
        {
            direction.X -= dirVel;
            moving = true;
        }
        if (Input.IsActionPressed("move_back"))
        {
            direction.Z += dirVel;
            moving = true;
        }
        if (Input.IsActionPressed("move_forward"))
        {
            direction.Z -= dirVel;
            moving = true;
        }
        
        if (Input.IsActionPressed("crouch"))
        {
            crouch = true;
            
        }


        Vector3 floorNormal = this.GetFloorNormal();       
        

        Vector3 projected = direction.Dot(floorNormal) / floorNormal.Dot(floorNormal) * floorNormal;

        projected = direction - projected;


        DebugDraw3D.DrawLine(this.GlobalPosition + Vector3.Up * 3, this.GlobalPosition + Vector3.Up * 3 + projected, Colors.Red);


        if (direction != Vector3.Zero)
        {

            Vector3 flip = new Vector3(-direction.X, 0, -direction.Z);
            flip = flip.Normalized();

        
            Vector3 a = Vector3.Forward;
            Vector3 b = flip;
            float angle = a.SignedAngleTo(b,Vector3.Up);
            float lerpAngle = Mathf.LerpAngle(NodeThing.Rotation.Y,angle,10* (float)delta);
            NodeThing.Rotation = new Vector3(NodeThing.Rotation.X,lerpAngle,NodeThing.Rotation.Z);
          
        
            
            
        }
        if(crouch && !jumped){
                
            NodeThing.Scale = new Vector3(1,0.5f,1);
            collisionPos.Y = 0.5f;
        }
        if(!crouch){
            float orgPos = 1.5f;
            collisionPos.Y = orgPos;
        }
        CollisionShape.Position = collisionPos;
        if(moving){
            if(!Input.IsActionPressed("move_right") && !Input.IsActionPressed("move_left") 
            && !Input.IsActionPressed("move_forward") && !Input.IsActionPressed("move_back")){
                moving = false;
            }
            if (mode == 1){
                MoveTimer = 1;
            }
            if(MoveTimer<1 && mode ==2){
                MoveTimer += 0.01f;
            }
            if(MoveTimer<1 && mode ==3){
                MoveTimer += 0.01f;
            }
            if(MoveTimer > 1){
                MoveTimer = 1;
            }
            
            //GD.Print(direction.Length());
            //GD.Print(dirVel);
            _targetVelocity.X = direction.X * Speed* dirVel;
            _targetVelocity.Z = direction.Z * Speed* dirVel;
            
            
        }
        if(!moving){
            
            if(MoveTimer>0 && mode == 1){
                MoveTimer = 0f;
                
            }
            if(MoveTimer>0 && mode == 2){
                MoveTimer -= 0.05f;
                
            }
            if(MoveTimer>0 && mode == 3){
                MoveTimer -= 0.05f;
                dirVel = Movement.Sample(MoveTimer);
                
            }
            if (MoveTimer <0){
                MoveTimer = 0;
            }
            _targetVelocity.X = latestInput.X * Speed * dirVel;
            _targetVelocity.Z = latestInput.Z * Speed * dirVel;
        }
        
        if(direction.Length() > 0.5f){
            latestInput = direction; 
        }
        
        
        
       
       
        
        if (!IsOnFloor()) 
        {
            timer++;
            if(Input.IsActionPressed("crouch")){
                _targetVelocity.Y = NodeThing.Position.Y;
            }else{
                _targetVelocity.Y -= FallAcceleration * (float)delta;
            }
           
            
            
        }
        if(Input.IsActionPressed("jump") && !jumped ){
            jumped = true;
            released = false;
        }
        
        
        
        if(jumped){
            
            NodeThing.Scale = new Vector3(1,0.5f,1);
            if(Input.IsActionJustReleased("jump")){
                released = true;
            }
            if(timer2 < 2 && !released){
                timer2 += 0.05f;
            }
        }
        
        
        if (timer < 15 && released)
        {
            
            released = false;
            jumped=false;
            NodeThing.Scale = new Vector3(1,timer2,1);
            _targetVelocity.Y = JumpImpulse * timer2 ;
            
        }
    
     
        Velocity = _targetVelocity;
        MoveAndSlide();
    }

}