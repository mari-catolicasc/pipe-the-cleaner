using Godot;
using System;

namespace PipeTheCleaner.scripts
{
    public partial class FollowPlayer : CharacterBody2D
    {
        [Export] public NodePath DetectionAreaPath;
        [Export] public NodePath PathFollowPath;

        [Export] public float PatrolSpeed = 60.0f;
        [Export] public float ChaseSpeed = 100.0f;

        private AnimatedSprite2D _animatedSprite;
        private Area2D _detectionArea;
        private PathFollow2D _pathFollow;

        private Node2D _targetPlayer = null;
        private bool _isChasing = false;

        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            _animatedSprite.Play("swim");

            // Configura a área de detecção
            if (DetectionAreaPath != null && !DetectionAreaPath.IsEmpty)
                _detectionArea = GetNode<Area2D>(DetectionAreaPath);
            else
                _detectionArea = GetNode<Area2D>("DetectionArea");

            // Configura a referência do PathFollow2D
            if (PathFollowPath != null && !PathFollowPath.IsEmpty)
                _pathFollow = GetNode<PathFollow2D>(PathFollowPath);

            if (_detectionArea != null)
            {
                _detectionArea.BodyEntered += OnDetectionAreaBodyEntered;
                _detectionArea.BodyExited += OnDetectionAreaBodyExited;
            }
            else
            {
                GD.PrintErr($"Erro: Nó de detecção não encontrado no tubarão: {Name}");
            }

            if (_pathFollow == null)
            {
                GD.PrintErr($"Aviso em {Name}: PathFollow2D não foi configurado. Ele ficará parado se não estiver caçando.");
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            // Evita que ele vire de cabeça para baixo
            GlobalRotation = 0;

            Vector2 velocity = Velocity;

            if (_isChasing && GodotObject.IsInstanceValid(_targetPlayer))
            {
                // Perseguir o Player
                Vector2 direction = (_targetPlayer.GlobalPosition - GlobalPosition).Normalized();
                velocity = direction * ChaseSpeed;

                GerenciarFlip(direction.X);
            }
            else if (_pathFollow != null)
            {
                // Patrulhar a Rota (Seguir o PathFollow2D)

                // Faz o guia invisível avançar pelo caminho continuamente
                _pathFollow.Progress += PatrolSpeed * (float)delta;

                // O tubarão tenta nadar na direção de onde o guia do caminho está agora
                Vector2 targetPos = _pathFollow.GlobalPosition;
                Vector2 direction = (targetPos - GlobalPosition).Normalized();

                float distanceToTarget = GlobalPosition.DistanceTo(targetPos);

                // Se ele estiver muito longe da rota (por exemplo, logo após o player fugir),
                // ele nada mais rápido em direção ao caminho para se readequar
                if (distanceToTarget > 20.0f)
                {
                    velocity = direction * ChaseSpeed; // Corre para voltar à rota
                }
                else
                {
                    velocity = direction * PatrolSpeed; // Velocidade normal de patrulha
                }

                GerenciarFlip(direction.X);
            }
            else
            {
                // Caso não tenha caminho nem player, fica parado
                velocity = Vector2.Zero;
            }

            Velocity = velocity;
            MoveAndSlide();
        }

        private void GerenciarFlip(float directionX)
        {
            if (Mathf.Abs(directionX) > 0.1f)
            {
                // Inverte horizontalmente baseado no movimento do eixo X
                _animatedSprite.FlipH = directionX < 0;
            }
        }

        private void OnDetectionAreaBodyEntered(Node2D body)
        {
            if (body.Name == "Player")
            {
                GD.Print("Tubarão-Tigre: Ignorando patrulha para caçar o Pipe!");
                _targetPlayer = body;
                _isChasing = true;
            }
        }

        private void OnDetectionAreaBodyExited(Node2D body)
        {
            if (body == _targetPlayer)
            {
                GD.Print("Tubarão-Tigre: Player escapou. Retornando ao circuito de patrulha.");
                _targetPlayer = null;
                _isChasing = false;
            }
        }
    }
}