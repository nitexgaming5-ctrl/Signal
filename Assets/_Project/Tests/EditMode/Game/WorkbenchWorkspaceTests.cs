using NUnit.Framework;
using UnityEngine;

namespace Signal.Game.Tests
{
    public sealed class WorkbenchWorkspaceTests
    {
        [Test]
        public void OpeningWorkspaceLocksMovementAndClosingRestoresIt()
        {
            var workspaceHost = new GameObject("WorkspaceHost");
            var player = new GameObject("Player");
            player.AddComponent<Rigidbody2D>();
            PlayerMotor2D motor = player.AddComponent<PlayerMotor2D>();

            try
            {
                WorkbenchWorkspace workspace = workspaceHost.AddComponent<WorkbenchWorkspace>();
                workspace.Bind(motor);

                workspace.Open();

                Assert.That(workspace.IsOpen, Is.True);
                Assert.That(motor.enabled, Is.False);
                Assert.That(workspaceHost.transform.Find("WorkbenchWorkspaceCanvas"), Is.Not.Null);

                workspace.Close();

                Assert.That(workspace.IsOpen, Is.False);
                Assert.That(motor.enabled, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(workspaceHost);
                Object.DestroyImmediate(player);
            }
        }
    }
}