# Gym Simulator

Projeto Introdução ao Unity — Insper 2026.2 — Gabriel Nasser

Você tem 3 minutos antes da academia fechar para completar a ficha de treino: 8 exercícios, cada um com um minigame. Cuidado com o marombeiro que esbarra em você e com o chato do "vai usar?" que rouba seu tempo.

**Jogar:** [itch.io](LINK_DO_ITCH)

## Controles


| Ação             | Teclado      | Controle (Xbox)            |
| ---------------- | ------------ | -------------------------- |
| Andar            | WASD / setas | Analógico esquerdo / D-pad |
| Usar máquina     | E / Enter    | A                          |
| Pausar           | Esc / P      | Start                      |
| Sair do minigame | Esc          | B                          |


## Minigames


| Exercício          | Mecânica                                                                               | Controles                   |
| ------------------ | -------------------------------------------------------------------------------------- | --------------------------- |
| Supino             | Aperte quando o anel fechar (estilo osu!)                                              | Espaço / Z / X / Clique / A |
| Agachamento        | Segure pra descer e solte na profundidade certa                                        | Segurar e soltar Espaço / A |
| Desenvolvimento    | Aperte várias vezes pra empurrar contra a gravidade, passando pelo ponto de travamento | Espaço / A (mash)           |
| Levantamento Terra | Segure pra firmar a pegada e depois puxe, sem deixar a lombar superaquecer             | Segurar, depois mash        |
| Tríceps            | Mantenha o cursor dentro da faixa verde que se move                                    | Segurar e soltar Espaço / A |
| Leg Curl           | Digite a sequência de setas antes do tempo acabar                                      | WASD / setas / D-pad        |
| Rosca              | Alterne esquerda e direita no ritmo; apertar os dois juntos é "roubar"                 | A / D, setas, LB / RB       |
| Panturrilha        | Pule as anilhas que vêm rolando (segure pra pular mais alto)                           | Espaço / A                  |


## Mecânicas

- **Tempo:** cronômetro regressivo de 3:00. Se zerar, a academia fecha e você perde. O tempo que sobrar vira bônus de score. O tempo final aparece na tela de fim.
- **Energia (stamina):** cada série gasta energia. Ela regenera devagar, e os shakes de whey espalhados pela academia recuperam mais.
- **Inimigos:**
  - Marombeiros patrulham rotas fixas e tiram energia ao esbarrar.
  - O chato do "vai usar?" persegue o jogador quando ele chega perto e rouba 5 segundos.
- **Score:** PERFECT vale 300 e GOOD vale 100. Shake vale 50, e o tempo que sobrar também conta. O recorde fica salvo.

## Estrutura

- `Assets/Scripts`: lógica do jogo (`GameManager`, `PlayerMovement`, `GymMachine`, `Hud`, inimigos, minigames).
- `Assets/Editor/SceneBuilder.cs`: monta as cenas `Menu` e `Game` pelo menu **Tools → Build Game Scene**.
  - Cuidado: rodar de novo sobrescreve edições manuais nas cenas.
- `Assets/Editor/WebGLBuilder.cs`: gera a build WebGL em `Build/WebGL` pelo menu **Tools → Build WebGL**.

## Créditos

- Engine: Unity 6 (template Universal 2D)
- Base: tutorial "Introdução Rápida" da disciplina
- Sprites em pixel art, efeitos sonoros e música: gerados por script, feitos para este projeto
- Fonte: LegacyRuntime (Arial), embutida no Unity
- Código desenvolvido com auxílio de IA

