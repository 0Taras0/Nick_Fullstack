import { Component, input, output } from '@angular/core';
import type { GifteePersonalInfoItem } from '../../../app.models';
import {
  ButtonText,
  ModalSubtitle,
  ModalTitle,
  PictureName,
} from '../../../app.enum';
import { CommonModalTemplate } from '../../../shared/components/modal/common-modal-template/common-modal-template';
import { PersonalInfo } from '../personal-info/personal-info';

@Component({
  selector: 'app-delete-user-modal',
  imports: [CommonModalTemplate, PersonalInfo],
  templateUrl: './delete-user-modal.html',
  styleUrl: './delete-user-modal.scss',
})
export class DeleteUserModal {
  readonly personalInfo = input.required<GifteePersonalInfoItem[]>();
  readonly roomLink = input.required<string>();

  readonly closeModal = output<void>();
  readonly buttonAction = output<void>();

  public readonly pictureName = PictureName.Exclamation;
  public readonly title = ModalTitle.DeleteUser;
  public readonly buttonText = ButtonText.Delete;
  public readonly subtitle = ModalSubtitle.DeleteUser;

  public onCloseModal(): void {
    this.closeModal.emit();
  }

  public onActionButtonClick(): void {
    this.buttonAction.emit();
  }
}
