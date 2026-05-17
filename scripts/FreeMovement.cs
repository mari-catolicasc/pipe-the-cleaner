using Godot;
using System;

namespace PipeTheCleaner.scripts
{
    public partial class FreeMovement : Area2D
    {
        [Export] public float Speed = 90.0f;

        private PathFollow2D _pathFollow;
        private AnimatedSprite2D _animatedSprite;
        private float _lastProgress;

        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            _animatedSprite.Play("walk");

            // Obtém o pai, que deve ser o PathFollow2D
            _pathFollow = GetParent<PathFollow2D>();

            if (_pathFollow == null)
            {
                GD.PrintErr("Erro: O pai deste nó precisa ser um PathFollow2D!");
            }
            else
            {
                _lastProgress = _pathFollow.Progress;
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_pathFollow == null) return;

            // Avança o PathFollow2D ao longo do caminho
            _pathFollow.Progress += Speed * (float)delta;

            // Identifica a direção do movimento baseada na mudança de posição real
            // (Funciona mesmo se o Path2D fizer curvas acentuadas ou loops)
            Vector2 currentPos = GlobalPosition;

            // Para atualizar as animações baseadas na direção para onde ele está indo:
            GerenciarAnimacao();
        }

        private void GerenciarAnimacao()
        {
            GlobalRotation = 0;

            float angle = _pathFollow.Rotation;
            Vector2 directionVector = Vector2.FromAngle(angle);

            if (directionVector.X < -0.1f)
            {
                // Indo para a esquerda -> espelha horizontalmente
                _animatedSprite.FlipH = true;
                _animatedSprite.Play("walk");
            }
            else if (directionVector.X > 0.1f)
            {
                // Indo para a direita -> volta ao normal
                _animatedSprite.FlipH = false;
                _animatedSprite.Play("walk");
            }
            else if (directionVector.Y < -0.1f)
            {
                // Indo para cima
                _animatedSprite.FlipV = false;
                _animatedSprite.Play("up");
            }
            else if (directionVector.Y > 0.1f)
            {
                // Indo para baixo
                _animatedSprite.FlipV = true;
                _animatedSprite.Play("up");
            }
        }
    }
}