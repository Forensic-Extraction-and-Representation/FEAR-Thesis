import { Attributes } from "graphology-types";

import { Settings } from "sigma/settings";
import { NodeDisplayData, PartialButFor } from "sigma/types";

export function drawCenteredNodeLabel<
    N extends Attributes = Attributes,
    E extends Attributes = Attributes,
    G extends Attributes = Attributes,
>(
    context: CanvasRenderingContext2D,
    data: PartialButFor<NodeDisplayData, "x" | "y" | "size" | "label" | "color">,
    settings: Settings<N, E, G>,
): void {
    if (!data.label) return;
    const size = settings.labelSize,
        font = settings.labelFont,
        weight = settings.labelWeight,
        color = settings.labelColor.attribute
            ? data[settings.labelColor.attribute] || settings.labelColor.color || "#000"
            : settings.labelColor.color;

    //context.fillStyle = color;
    context.font = `${weight} ${size}px ${font}`;
    const textMetrics = context.measureText(data.label);

    // Size is the size of the node, this places the label to the right of the node
    // data.x + data.size + 3 is the x position of the label (3px right of the node)
    // data.y + size / 3 is the y position of the label (centered vertically-ish)
    context.fillText(data.label, data.x - (textMetrics.width / 2), data.y + size / 3);
}