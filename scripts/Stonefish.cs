using Godot;
using System;

namespace PipeTheCleaner.scripts
{
    public partial class Stonefish : Area2D
    {
        // Ao exportar a NodePath, você define o nó direto pelo Inspetor do Godot!
        [Export] public NodePath DetectionAreaPath;
        [Export] public float Speed = 90.0f;

        private AnimatedSprite2D _animatedSprite;
        private Area2D _detectionArea;
        private bool _isBiting = false;
        private float _biteTimer = 0.0f;
        private const float ReturnToIdleTime = 5.0f;

        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

            // Se você definiu o caminho no Inspetor, ele usa. Se esqueceu, ele tenta buscar pelo nome padrão.
            if (DetectionAreaPath != null && !DetectionAreaPath.IsEmpty)
            {
                _detectionArea = GetNode<Area2D>(DetectionAreaPath);
            }
            else
            {
                _detectionArea = GetNode<Area2D>("DetectionArea");
            }

            _animatedSprite.Play("idle");

            // Conecta o sinal de forma segura
            if (_detectionArea != null)
            {
                _detectionArea.BodyEntered += OnDetectionAreaBodyEntered;
            }
            else
            {
                GD.PrintErr($"Erro crítico em {Name}: Nó de detecção não foi atribuído ou encontrado!");
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_isBiting)
            {
                _biteTimer += (float)delta;

                if (_biteTimer >= ReturnToIdleTime)
                {
                    VoltarParaIdle();
                }
            }
        }

        private void OnDetectionAreaBodyEntered(Node2D body)
        {
            if (body.Name == "Player" && !_isBiting)
            {
                Abocanhar(body);
            }
        }

        private void Abocanhar(Node2D body)
        {
            _isBiting = true;
            _biteTimer = 0.0f;

            GD.Print("Stonefish: Atacando o Pipe!");
            _animatedSprite.Play("bite");

            if (OverlapsBody(body))
            {
                GD.Print("Stonefish: Dano confirmado no corpo a corpo!");
                // body.Call("TomarDano", 10); // Execute a sua função de dano aqui com segurança
            }
            else
            {
                GD.Print("Stonefish: Errou o bote! O player estava na área de visão, mas longe do corpo.");
            }
        }

        private void VoltarParaIdle()
        {
            _isBiting = false;
            _biteTimer = 0.0f;

            GD.Print("Stonefish: Voltou a se camuflar.");
            _animatedSprite.Play("idle");
        }
    }
}