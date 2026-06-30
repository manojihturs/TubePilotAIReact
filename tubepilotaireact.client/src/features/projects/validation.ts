import type { ProjectPayload } from '../../types/project';

export function validateProject(payload: ProjectPayload) {
  const errors: string[] = [];

  addLengthError(errors, 'Name', payload.name, 3, 160, true);
  addLengthError(errors, 'Description', payload.description, 0, 1200, false);
  addLengthError(errors, 'Owner name', payload.ownerName, 2, 120, true);

  if (payload.startDate && payload.dueDate && payload.dueDate < payload.startDate) {
    errors.push('Due date cannot be earlier than start date.');
  }

  if (payload.budget < 0) {
    errors.push('Budget cannot be negative.');
  }

  const normalizedTags = payload.tags.map((tag) => tag.trim()).filter(Boolean);
  const duplicateTags = normalizedTags.filter(
    (tag, index) => normalizedTags.findIndex((candidate) => candidate.toLowerCase() === tag.toLowerCase()) !== index,
  );

  if (duplicateTags.length > 0) {
    errors.push(`Tags must be unique: ${Array.from(new Set(duplicateTags)).join(', ')}.`);
  }

  for (const tag of payload.tags) {
    if (!tag.trim()) {
      errors.push('Tags cannot include blank values.');
    }

    if (tag.length > 60) {
      errors.push(`Tag "${tag}" cannot be longer than 60 characters.`);
    }
  }

  return errors;
}

function addLengthError(
  errors: string[],
  field: string,
  value: string | null | undefined,
  min: number,
  max: number,
  required: boolean,
) {
  const trimmed = value?.trim() ?? '';

  if (!trimmed) {
    if (required) {
      errors.push(`${field} is required.`);
    }

    return;
  }

  if (trimmed.length < min) {
    errors.push(`${field} must be at least ${min} characters.`);
  }

  if (trimmed.length > max) {
    errors.push(`${field} cannot be longer than ${max} characters.`);
  }
}
